using System;
using System.IO;
using System.Reflection;
using Autofac;
using CaptainHook.Api.Core;
using CaptainHook.Api.Helpers;
using CaptainHook.Domain.Handlers.Subscribers;
using CaptainHook.Domain.RequestValidators;
using CaptainHook.Storage.Cosmos;
using Eshopworld.Core;
using Eshopworld.Data.CosmosDb;
using Eshopworld.DevOps;
using Eshopworld.Telemetry;
using Eshopworld.Telemetry.Configuration;
using Eshopworld.Telemetry.Processors;
using Eshopworld.Web;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace CaptainHook.Api
{
    /// <summary>
    /// Startup class for ASP.NET runtime
    /// </summary>
    public class Startup
    {
        private readonly string _instrumentationKey;
        private readonly IBigBrother _bb;
        private readonly IConfigurationRoot _configuration;
        private bool UseOpenApiV2 => true;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="env">hosting environment</param>
        public Startup(IWebHostEnvironment env)
        {
            _configuration = EswDevOpsSdk.BuildConfiguration(env.ContentRootPath, env.EnvironmentName);
            _instrumentationKey = _configuration["InstrumentationKey"];
            _bb = BigBrother.CreateDefault(_instrumentationKey, _instrumentationKey);
        }

        /// <summary>
        /// ConfigureServices is where you register dependencies. This gets
        /// called by the runtime before the ConfigureContainer method, below.
        /// </summary>
        /// <remarks>See https://docs.autofac.org/en/latest/integration/aspnetcore.html#asp-net-core-3-0-and-generic-hosting</remarks>
        /// <param name="builder">The <see cref="ContainerBuilder"/> to configure</param>
        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.ConfigureTelemetryKeys(_instrumentationKey, _instrumentationKey);
            builder.RegisterModule<TelemetryModule>();

            builder.RegisterType<SuccessfulProbeFilterCriteria>()
                .As<ITelemetryFilterCriteria>();

            builder.RegisterModule<CosmosDbStorageModule>();
            builder.RegisterModule<CosmosDbModule>();

            // Mediator itself
            builder
                .RegisterType<Mediator>()
                .As<IMediator>()
                .InstancePerLifetimeScope();

            // request & notification handlers
            builder.Register<ServiceFactory>(context =>
            {
                var c = context.Resolve<IComponentContext>();
                return t => c.Resolve(t);
            });

            // finally register our custom code (individually, or via assembly scanning)
            // - requests & handlers as transient, i.e. InstancePerDependency()
            // - pre/post-processors as scoped/per-request, i.e. InstancePerLifetimeScope()
            // - behaviors as transient, i.e. InstancePerDependency()
            builder.RegisterAssemblyTypes(typeof(AddSubscriberRequestHandler).GetTypeInfo().Assembly).AsImplementedInterfaces(); // via assembly scan
            //builder.RegisterType<MyHandler>().AsImplementedInterfaces().InstancePerDependency();          // or individually

            // For all the validators, register them with dependency injection as scoped
            AssemblyScanner.FindValidatorsInAssembly(typeof(AddSubscriberRequestValidator).Assembly)
              //.ForEach(item => services.AddScoped(item.InterfaceType, item.ValidatorType));
              .ForEach(item => builder.RegisterType(item.ValidatorType).As(item.InterfaceType));

            builder.RegisterGeneric(typeof(ValidatorPipelineBehavior<,>)).As(typeof(IPipelineBehavior<,>));
        }

        /// <summary>
        /// configure services to be used by the asp.net runtime
        /// </summary>
        /// <param name="services">service collection</param>
        public void ConfigureServices(IServiceCollection services)
        {
            try
            {
                services.AddApplicationInsightsTelemetry(_instrumentationKey);

                var serviceConfiguration = new ServiceConfigurationOptions();
                _configuration.GetSection("ServiceConfigurationOptions").Bind(serviceConfiguration);

                services.AddControllers(options =>
                {
                    var policy = ScopePolicy.Create(serviceConfiguration.RequiredScopes.ToArray());

                    var filter = EnvironmentHelper.IsInFabric ?
                        (IFilterMetadata)new AuthorizeFilter(policy) :
                        new AllowAnonymousFilter();

                    options.Filters.Add(filter);

                }).AddNewtonsoftJson();

                services.AddApiVersioning(options => options.AssumeDefaultVersionWhenUnspecified = true);
                services.AddHealthChecks();

                //services
                //    .AddMvc()
                //    .ConfigureApiBehaviorOptions(options =>
                //    {
                //        options.InvalidModelStateResponseFactory = c =>
                //        {
                //            var errors = c.ModelState.Values.Where(v => v.Errors.Count > 0)
                //                .SelectMany(v => v.Errors)
                //                .Select(v => v.ErrorMessage);

                //            return new BadRequestObjectResult(new
                //            {
                //                Message = "Request is invalid",
                //                Failures = errors
                //            });
                //        };
                //    })
                //    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<SubscriberDtoValidator>());

                // Get XML documentation
                var path = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");

                // If not generated throw an event but it's not going to stop the app from starting
                if (!File.Exists(path))
                {
                    BigBrother.Write(new Exception("Swagger XML document has not been included in the project"));
                }
                else
                {
                    services.AddSwaggerGen(c =>
                    {
                        c.IncludeXmlComments(path);
                        c.SwaggerDoc("v1", new OpenApiInfo { Version = "v1", Title = "CaptainHook.Api" });
                        c.CustomSchemaIds(x => x.FullName);
                        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                        {
                            In = ParameterLocation.Header,
                            Description = "Please insert JWT with Bearer into field",
                            Name = "Authorization",
                            Type = UseOpenApiV2 ? SecuritySchemeType.ApiKey : SecuritySchemeType.Http,
                            Scheme = "bearer",
                            BearerFormat = "JWT",
                        });

                        c.AddSecurityRequirement(new OpenApiSecurityRequirement
                        {
                            {
                                new OpenApiSecurityScheme
                                {
                                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
                                },
                                new string[0]
                            }
                        });

                        c.OperationFilter<OperationIdFilter>();
                    });
                }

                services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddIdentityServerAuthentication(x =>
                {
                    x.ApiName = serviceConfiguration.ApiName;
                    x.ApiSecret = serviceConfiguration.ApiSecret;
                    x.Authority = serviceConfiguration.Authority;
                    x.RequireHttpsMetadata = serviceConfiguration.IsHttps;
                    // To include telemetry Install-Package EShopworld.Security.Services.Telemetry -Source https://eshopworld.myget.org/F/github-dev/api/v3/index.json
                    // See https://eshopworld.visualstudio.com/evo-core/_git/security-services-telemetry?path=%2FREADME.md&_a=preview
                    // x.AddJwtBearerEventsTelemetry(bb); 
                });
                services.AddAuthorization(options =>
                {
                    options.AddPolicy(Constants.AuthorisationPolicies.SubscribersAccess,
                        policy => policy.RequireScope(Constants.AuthorisationScopes.ApiAllAccess));
                });
            }
            catch (Exception e)
            {
                _bb.Publish(e.ToExceptionEvent());
                throw;
            }
        }

        /// <summary>
        /// configure asp.net pipeline
        /// </summary>
        /// <param name="app">application builder</param>
        /// <param name="env">environment</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseBigBrotherExceptionHandler();
            app.UseSwagger(o =>
            {
                o.RouteTemplate = "swagger/{documentName}/swagger.json";
                o.SerializeAsV2 = UseOpenApiV2;
            });
            app.UseSwaggerUI(o =>
            {
                o.SwaggerEndpoint("v1/swagger.json", "CaptainHook.Api");
                o.RoutePrefix = "swagger";
            });

            app.UseRouting();

#if OAUTH_OFF_MODE
            app.UseFakeAuthentication();
#else
            app.UseAuthentication();
#endif
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
