using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace CaptainHook.Api
{
    public class SwaggerHostFactory
    {
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