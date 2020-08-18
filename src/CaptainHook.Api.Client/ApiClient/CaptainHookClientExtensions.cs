// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace CaptainHook.Api.Client
{
    using Microsoft.Rest;
    using Models;
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
            /// Reloads configuration for Captain Hook
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            public static void Reload(this ICaptainHookClient operations)
            {
                operations.ReloadAsync().GetAwaiter().GetResult();
            }

            /// <summary>
            /// Reloads configuration for Captain Hook
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task ReloadAsync(this ICaptainHookClient operations, CancellationToken cancellationToken = default(CancellationToken))
            {
                (await operations.ReloadWithHttpMessagesAsync(null, cancellationToken).ConfigureAwait(false)).Dispose();
            }

            /// <summary>
            /// Reloads configuration for Captain Hook
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='customHeaders'>
            /// Headers that will be added to request.
            /// </param>
            public static HttpOperationResponse ReloadWithHttpMessages(this ICaptainHookClient operations, Dictionary<string, List<string>> customHeaders = null)
            {
                return operations.ReloadWithHttpMessagesAsync(customHeaders, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
            }

            /// <summary>
            /// Returns whether an existing configuration reload request is in progress
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            public static void GetStatus(this ICaptainHookClient operations)
            {
                operations.GetStatusAsync().GetAwaiter().GetResult();
            }

            /// <summary>
            /// Returns whether an existing configuration reload request is in progress
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task GetStatusAsync(this ICaptainHookClient operations, CancellationToken cancellationToken = default(CancellationToken))
            {
                (await operations.GetStatusWithHttpMessagesAsync(null, cancellationToken).ConfigureAwait(false)).Dispose();
            }

            /// <summary>
            /// Returns whether an existing configuration reload request is in progress
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='customHeaders'>
            /// Headers that will be added to request.
            /// </param>
            public static HttpOperationResponse GetStatusWithHttpMessages(this ICaptainHookClient operations, Dictionary<string, List<string>> customHeaders = null)
            {
                return operations.GetStatusWithHttpMessagesAsync(customHeaders, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
            }

            /// <summary>
            /// Insert or update a web hook
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='eventName'>
            /// Event name
            /// </param>
            /// <param name='subscriberName'>
            /// Subscriber name
            /// </param>
            /// <param name='body'>
            /// Webhook configuration
            /// </param>
            public static object PutWebhook(this ICaptainHookClient operations, string eventName, string subscriberName, CaptainHookContractEndpointDto body = default(CaptainHookContractEndpointDto))
            {
                return operations.PutWebhookAsync(eventName, subscriberName, body).GetAwaiter().GetResult();
            }

            /// <summary>
            /// Insert or update a web hook
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='eventName'>
            /// Event name
            /// </param>
            /// <param name='subscriberName'>
            /// Subscriber name
            /// </param>
            /// <param name='body'>
            /// Webhook configuration
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<object> PutWebhookAsync(this ICaptainHookClient operations, string eventName, string subscriberName, CaptainHookContractEndpointDto body = default(CaptainHookContractEndpointDto), CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.PutWebhookWithHttpMessagesAsync(eventName, subscriberName, body, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <summary>
            /// Insert or update a web hook
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='eventName'>
            /// Event name
            /// </param>
            /// <param name='subscriberName'>
            /// Subscriber name
            /// </param>
            /// <param name='body'>
            /// Webhook configuration
            /// </param>
            /// <param name='customHeaders'>
            /// Headers that will be added to request.
            /// </param>
            public static HttpOperationResponse<object> PutWebhookWithHttpMessages(this ICaptainHookClient operations, string eventName, string subscriberName, CaptainHookContractEndpointDto body = default(CaptainHookContractEndpointDto), Dictionary<string, List<string>> customHeaders = null)
            {
                return operations.PutWebhookWithHttpMessagesAsync(eventName, subscriberName, body, customHeaders, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
            }

            /// <summary>
            /// Insert or update a subscriber
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='eventName'>
            /// Event name
            /// </param>
            /// <param name='subscriberName'>
            /// Subscriber name
            /// </param>
            /// <param name='body'>
            /// Webhook configuration
            /// </param>
            public static object PutSuscriber(this ICaptainHookClient operations, string eventName, string subscriberName, CaptainHookContractSubscriberDto body = default(CaptainHookContractSubscriberDto))
            {
                return operations.PutSuscriberAsync(eventName, subscriberName, body).GetAwaiter().GetResult();
            }

            /// <summary>
            /// Insert or update a subscriber
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='eventName'>
            /// Event name
            /// </param>
            /// <param name='subscriberName'>
            /// Subscriber name
            /// </param>
            /// <param name='body'>
            /// Webhook configuration
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<object> PutSuscriberAsync(this ICaptainHookClient operations, string eventName, string subscriberName, CaptainHookContractSubscriberDto body = default(CaptainHookContractSubscriberDto), CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.PutSuscriberWithHttpMessagesAsync(eventName, subscriberName, body, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <summary>
            /// Insert or update a subscriber
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='eventName'>
            /// Event name
            /// </param>
            /// <param name='subscriberName'>
            /// Subscriber name
            /// </param>
            /// <param name='body'>
            /// Webhook configuration
            /// </param>
            /// <param name='customHeaders'>
            /// Headers that will be added to request.
            /// </param>
            public static HttpOperationResponse<object> PutSuscriberWithHttpMessages(this ICaptainHookClient operations, string eventName, string subscriberName, CaptainHookContractSubscriberDto body = default(CaptainHookContractSubscriberDto), Dictionary<string, List<string>> customHeaders = null)
            {
                return operations.PutSuscriberWithHttpMessagesAsync(eventName, subscriberName, body, customHeaders, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
            }

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
            /// Retrieve all subscribers from current configuration
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            public static void GetAll(this ICaptainHookClient operations)
            {
                operations.GetAllAsync().GetAwaiter().GetResult();
            }

            /// <summary>
            /// Retrieve all subscribers from current configuration
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task GetAllAsync(this ICaptainHookClient operations, CancellationToken cancellationToken = default(CancellationToken))
            {
                (await operations.GetAllWithHttpMessagesAsync(null, cancellationToken).ConfigureAwait(false)).Dispose();
            }

            /// <summary>
            /// Retrieve all subscribers from current configuration
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='customHeaders'>
            /// Headers that will be added to request.
            /// </param>
            public static HttpOperationResponse GetAllWithHttpMessages(this ICaptainHookClient operations, Dictionary<string, List<string>> customHeaders = null)
            {
                return operations.GetAllWithHttpMessagesAsync(customHeaders, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
            }

    }
}
