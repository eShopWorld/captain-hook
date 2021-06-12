using System;
using System.Diagnostics;
using System.Threading;
using Autofac.Extensions.DependencyInjection;
using CaptainHook.Common;
using Eshopworld.Telemetry;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.ServiceFabric.Services.Runtime;
using Platform.AppInfrastructure.Web;

namespace CaptainHook.Api
{
    internal static class Program
    {
        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static void Main()
        {
            try
            {
                if (EnvironmentHelper.IsInFabric)
                {
                    // The ServiceManifest.XML file defines one or more service type names.
                    // Registering a service maps a service type name to a .NET type.
                    // When Service Fabric creates an instance of this service type,
                    // an instance of the class is created in this host process.

                    ServiceRuntime.RegisterServiceAsync(ServiceNaming.ApiServiceServiceType,
                        context => new CaptainHookApi(context)).GetAwaiter().GetResult();

                    ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, ServiceNaming.ApiServiceServiceType);

                    // Prevents this host process from terminating so services keeps running. 
                    Thread.Sleep(Timeout.Infinite);
                }
                else
                {
                    var host = Host
                        .CreateDefaultBuilder()
                        .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                        .ConfigureWebHostDefaults(w => w.UseStartup<Startup>())
                        .Build();

                    host.Run();
                }
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                BigBrother.Write(e);
                throw;
            }
        }
    }
}
