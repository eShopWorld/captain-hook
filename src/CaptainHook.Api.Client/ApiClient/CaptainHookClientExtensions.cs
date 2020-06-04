// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace CaptainHook.Api.Client
{
    using Microsoft.Rest;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Extension methods for CaptainHookClient.
    /// </summary>
    public static partial class CaptainHookClientExtensions
    {
            /// <summary>
            /// Returns a probe result
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            public static void GetProbe(this ICaptainHookClient operations)
            {
                operations.GetProbeAsync().GetAwaiter().GetResult();
            }

            /// <summary>
            /// Returns a probe result
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task GetProbeAsync(this ICaptainHookClient operations, CancellationToken cancellationToken = default(CancellationToken))
            {
                (await operations.GetProbeWithHttpMessagesAsync(null, cancellationToken).ConfigureAwait(false)).Dispose();
            }

            /// <summary>
            /// Returns a probe result
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='customHeaders'>
            /// Headers that will be added to request.
            /// </param>
            public static HttpOperationResponse GetProbeWithHttpMessages(this ICaptainHookClient operations, Dictionary<string, List<string>> customHeaders = null)
            {
                return operations.GetProbeWithHttpMessagesAsync(customHeaders, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
            }

            /// <summary>
            /// Refreshes configuration for the given event
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            public static void RefreshConfig(this ICaptainHookClient operations)
            {
                operations.RefreshConfigAsync().GetAwaiter().GetResult();
            }

            /// <summary>
            /// Refreshes configuration for the given event
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task RefreshConfigAsync(this ICaptainHookClient operations, CancellationToken cancellationToken = default(CancellationToken))
            {
                (await operations.RefreshConfigWithHttpMessagesAsync(null, cancellationToken).ConfigureAwait(false)).Dispose();
            }

            /// <summary>
            /// Refreshes configuration for the given event
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='customHeaders'>
            /// Headers that will be added to request.
            /// </param>
            public static HttpOperationResponse RefreshConfigWithHttpMessages(this ICaptainHookClient operations, Dictionary<string, List<string>> customHeaders = null)
            {
                return operations.RefreshConfigWithHttpMessagesAsync(customHeaders, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
            }

    }
}
