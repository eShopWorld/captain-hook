namespace CaptainHook.EventHandlerActor
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Common.Nasty;
    using Common.Telemetry;
    using Eshopworld.Core;
    using IdentityModel.Client;
    using Interfaces;
    using Microsoft.ServiceFabric.Actors;
    using Microsoft.ServiceFabric.Actors.Runtime;
    using Newtonsoft.Json;

    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    public class EventHandlerActor : Actor, IEventHandlerActor
    {
        private readonly IBigBrother _bigBrother;
        private readonly Dictionary<string, IHandler> _handlers;
        private IActorTimer _handleTimer;

        /// <summary>
        /// Initializes a new instance of EventHandlerActor
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public EventHandlerActor(ActorService actorService, ActorId actorId, IBigBrother bigBrother)
            : base(actorService, actorId)
        {
            _bigBrother = bigBrother;
            //register one handler per type such that the key is the object type and all the crap to handle it is contained in the handler
            this._handlers = new Dictionary<string, IHandler>();
            //todo inject httpclient for use so it get reused
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override async Task OnActivateAsync()
        {
            if ((await StateManager.TryGetStateAsync<MessageData>(nameof(EventHandlerActor))).HasValue)
            {
                // There's a message to handle, but we're not sure if it was fully handled or not, so we are going to handle it anyways

                _handleTimer = RegisterTimer(
                    InternalHandle,
                    null,
                    TimeSpan.FromMilliseconds(100),
                    TimeSpan.MaxValue);
            }
        }

        public async Task Handle(Guid handle, string payload, string type)
        {
            var messageData = new MessageData
            {
                Handle = handle,
                Payload = payload,
                Type = type
            };

            await StateManager.AddOrUpdateStateAsync(nameof(EventHandlerActor), messageData, (s, pair) => messageData);

            _handleTimer = RegisterTimer(
                InternalHandle,
                null,
                TimeSpan.FromMilliseconds(100),
                TimeSpan.MaxValue);
        }

        public async Task CompleteHandle(Guid handle)
        {
            await Task.Yield();
            throw new NotImplementedException();
        }

        private async Task InternalHandle(object _)
        {
            UnregisterTimer(_handleTimer);

            var messageData = await StateManager.TryGetStateAsync<MessageData>(nameof(EventHandlerActor));
            if (!messageData.HasValue)
            {
                //todo maybe a bit weird so we might want to event on this.
                return;
            }

            if (this._handlers.TryGetValue(messageData.Value.Type, out var handler))
            {
                await handler.MakeCall();

                _bigBrother.Publish(new MessageExecuted
                {
                    Payload = messageData.Value.Payload,
                    Type = messageData.Value.Type
                });
            }
            else
            {
                _bigBrother.Publish(new UnknownMessageType
                {
                    Payload = messageData.Value.Payload,
                    Type = messageData.Value.Type
                });
            }

            // TODO: HANDLE THE THING - PROBABLY PUT A TRANSACTION HERE AND SCOPE IT TO THE STATEMANAGER CALL

            await StateManager.RemoveStateAsync(nameof(EventHandlerActor));
        }
    }

    public class RetailerOrderConfirmationDomainEvent : DomainEvent
    {
        public Guid OrderCode { get; set; }

        public OrderConfirmationRequestDto OrderConfirmationRequestDto { get; set; }
    }


    public interface IHandler
    {
        void Register();

        Task MakeCall(MessageData data);
    }

    public class HandlerConfig
    {
        public string Uri { get; set; }

        public string Payload { get; set; }
    }

    public class MmEventHandler : BaseHandler
    {
        private readonly SecurityConfig _securityConfig;
        private readonly HandlerConfig _config;

        public MmEventHandler(SecurityConfig securityConfig, HandlerConfig config)
        {
            _securityConfig = securityConfig;
            _config = config;
        }

        public override void Register()
        {
            //todo feels like I don't really need this
            throw new NotImplementedException();
        }

        public override async Task MakeCall(MessageData data)
        {
            var token = await this.GetAuthorizationAccessToken(_securityConfig);

            var client = new HttpClient();
            client.SetBearerToken(token);

            //hit MaxMara
            //todo polly 429 and 503
            var response1 = await client.PostAsJsonAsync(_config.Uri, _config.Payload);

            if (response1.IsSuccessStatusCode)
            {
                //call checkout
                return;
            }

            var model = JsonConvert.DeserializeObject<RetailerOrderConfirmationDomainEvent>(data.Payload);

            var request = new HttpResponseDto
            {
                StatusCode = (int)response1.StatusCode,
                Content = response1
            };

            //hit platform
            var response2 = await client.PutAsJsonAsync($"api/v2/webhook/putorderconfirmationresult/{}", request);

             
        }
    }

    public abstract class BaseHandler : IHandler
    {
        protected async Task<string> GetAuthorizationAccessToken(SecurityConfig config)
        {
            var httpClient = new HttpClient();

            var response = await httpClient.RequestTokenAsync(new TokenRequest
            {
                Address = config.Uri,
                ClientId = config.ClientId,
                ClientSecret = config.ClientSecret,
            });
            return response.AccessToken;
        }

        public abstract void Register();

        public abstract Task MakeCall(MessageData data);
    }

    public class SecurityConfig
    {
        /// <summary>
        /// //todo put this in ci securityConfig/production securityConfig
        /// </summary>
        public string Uri { get; set; } = "https://security-sts.ci.eshopworld.net";

        public string ClientId { get; set; } = "esw.esa.hook.client";

        public string ClientSecret { get; set; } = "itdoesn'tmatterightnow";

        public string Scopes { get; set; }
    }
}
