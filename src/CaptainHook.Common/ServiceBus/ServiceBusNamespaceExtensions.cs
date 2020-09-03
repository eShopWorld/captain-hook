using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.ServiceBus.Fluent;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Rest;
using Polly;
using Polly.Retry;

namespace CaptainHook.Common.ServiceBus
{
    /// <summary>
    /// Contains extensions to the ServiceBus Fluent SDK: <see cref="Microsoft.Azure.Management.ServiceBus.Fluent"/>.
    /// </summary>
    public static class ServiceBusNamespaceExtensions
    {
       
    }
}