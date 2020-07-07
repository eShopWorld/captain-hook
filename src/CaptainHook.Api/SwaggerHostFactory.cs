using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace CaptainHook.Api
{
    /// <summary>
    /// Swagger factory
    /// </summary>
    public class SwaggerHostFactory
    {
        /// <summary>
        /// Create host
        /// </summary>
        /// <returns></returns>
        public static IHost CreateHost()
        {
            return Host
                .CreateDefaultBuilder()
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureWebHostDefaults(w => w.UseStartup<Startup>())
                .Build();
        }
    }
}