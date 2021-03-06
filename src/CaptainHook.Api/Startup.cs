using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using Autofac;
using CaptainHook.Api.Core;
using CaptainHook.Application;
using CaptainHook.Common.Configuration.KeyVault;
using CaptainHook.Contract;
using CaptainHook.Storage.Cosmos;
using Eshopworld.Core;
using Eshopworld.Data.CosmosDb.Extensions;
using Eshopworld.Telemetry;
using Eshopworld.Telemetry.Configuration;
using Eshopworld.Telemetry.Processors;
using Eshopworld.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using TelemetrySettings = Eshopworld.DevOps.TelemetrySettings;

namespace CaptainHook.Api
{
    /// <summary>
    /// Startup class for ASP.NET runtime
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        private readonly string _instrumentationKey;
        private readonly string _internalKey;
        private readonly IBigBrother _bb;
        private readonly IConfigurationRoot _configuration;
        private bool UseOpenApiV2 => true;

        /// <summary>
        /// Constructor
        /// </summary>
        public Startup()
        {
            _configuration = new ConfigurationBuilder()
                   .UseDefaultConfigs()
                   .AddKeyVaultSecrets(new Dictionary<string, string>
                   {
                        {"cm--ai-telemetry--instrumentation", "Telemetry:InstrumentationKey"},
                        {"cm--ai-telemetry--internal", "Telemetry:InternalKey"},
                        {"cm--cosmos-connection--esw-platform", "CosmosDB:ConnectionString"},
                   }).Build();

            var telemetrySettings = _configuration.GetSection("Telemetry").Get<TelemetrySettings>();
            _instrumentationKey = telemetrySettings.InstrumentationKey;
            _internalKey = telemetrySettings.InternalKey;

            _bb = BigBrother.CreateDefault(_instrumentationKey, _internalKey);
        }

        /// <summary>
        /// ConfigureServices is where you register dependencies. This gets
        /// called by the runtime before the ConfigureContainer method, below.
        /// </summary>
        /// <remarks>See https://docs.autofac.org/en/latest/integration/aspnetcore.html#asp-net-core-3-0-and-generic-hosting</remarks>
        /// <param name="builder">The <see cref="ContainerBuilder"/> to configure</param>
        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.ConfigureTelemetryKeys(_instrumentationKey, _internalKey);
            builder.RegisterKeyVaultSecretProvider(_configuration);

            builder.RegisterModule<TelemetryModule>();
            builder.RegisterType<SuccessfulProbeFilterCriteria>()
                .As<ITelemetryFilterCriteria>();
            builder.RegisterModule<ApplicationModule>();
            builder.RegisterModule<CosmosDbStorageModule>();
            builder.ConfigureCosmosDb(_configuration);
        }

        private static readonly AuthenticationDtoJsonConverter AuthenticationDtoJsonConverter = new AuthenticationDtoJsonConverter();

        /// <summary>
        /// configure services to be used by the asp.net runtime
        /// </summary>
        /// <param name="services">service collection</param>
        public void ConfigureServices(IServiceCollection services)
        {
            try
            {
                services.AddApplicationInsightsTelemetry(_instrumentationKey);
                services.AddApplicationInsightsTelemetryProcessor<TelemetryFilterProcessor>();

                var serviceConfiguration = _configuration.GetSection(nameof(ServiceConfigurationOptions)).Get<ServiceConfigurationOptions>();

                services.AddControllers(options =>
                    {
                        var policy =
                            ScopePolicy.Create(serviceConfiguration.RequiredScopes?.ToArray() ?? new string[0]);

                        var filter = EnvironmentHelper.IsInFabric
                            ? (IFilterMetadata)new AuthorizeFilter(policy)
                            : new AllowAnonymousFilter();

                        options.Filters.Add(filter);
                    })
                    .AddNewtonsoftJson(x => x.SerializerSettings.Converters.Add(AuthenticationDtoJsonConverter))
                    .ConfigureApiBehaviorOptions(options =>
                        options.SuppressMapClientErrors = true);

                services.AddApiVersioning(options => options.AssumeDefaultVersionWhenUnspecified = true);
                services.AddHealthChecks();

                // Get XML documentation
                var path = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");

                // If not generated throw an event but it's not going to stop the app from starting
                if (!File.Exists(path))
                {
                    BigBrother.Write(new Exception("Swagger XML document has not been included in the project"));
                }
                else
                {
                    services.AddSwaggerGenNewtonsoftSupport();
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
                        c.GeneratePolymorphicSchemas(type =>
                        {
                            if (type == typeof(AuthenticationDto))
                            {
                                return new[] {
                                    typeof(OidcAuthenticationDto),
                                    typeof(BasicAuthenticationDto),
                                    typeof(NoAuthenticationDto)
                                };
                            }
                            return Enumerable.Empty<Type>();
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
                    options.AddPolicy(Constants.AuthorisationPolicies.ReadSubscribers,
                        policy => policy.RequireScope(Constants.AuthorisationScopes.ApiReadSubscribers));
                    options.AddPolicy(Constants.AuthorisationPolicies.DefineSubscribers,
                       policy => policy.RequireScope(Constants.AuthorisationScopes.ApiDefineSubscribers));
                    options.AddPolicy(Constants.AuthorisationPolicies.DeleteSubscribers,
                       policy => policy.RequireScope(Constants.AuthorisationScopes.ApiDeleteSubscribers));
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
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
