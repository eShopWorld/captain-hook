using CaptainHook.Contract;
using CaptainHook.Domain.Entities;
using System.Linq;

namespace CaptainHook.Application.Infrastructure.Mappers
{
    public class EntityToDtoMapper : IEntityToDtoMapper
    {
        public SubscriberDto MapSubscriber(SubscriberEntity subscriberEntity)
        {
            return new SubscriberDto
            {
                Webhooks = MapWebhooks(subscriberEntity.Webhooks),
                Callbacks = MapWebhooks(subscriberEntity.Callbacks),
                DlqHooks = MapWebhooks(subscriberEntity.DlqHooks)
            };
        }

        public WebhooksDto MapWebhooks(WebhooksEntity entity)
        {
            return new WebhooksDto
            {
                SelectionRule = entity.SelectionRule,
                UriTransform = MapUriTransform(entity.UriTransform),
                Endpoints = entity.Endpoints.Select(MapEndpoint).ToList(),
                PayloadTransform = MapPayloadTransform(entity.PayloadTransform, entity.Type)
            };
        }

        public string MapPayloadTransform(string payloadTransform, WebhooksEntityType type)
        {
            if (type == WebhooksEntityType.Callbacks)
                return null;

            return payloadTransform switch
            {
                "$.Request" => "Request",
                "$.Response" => "Response",
                "$.OrderConfirmation" => "OrderConfirmation",
                "$.PlatformOrderConfirmation" => "PlatformOrderConfirmation",
                "$.EmptyCart" => "EmptyCart",
                _ => null
            };
        }

        public EndpointDto MapEndpoint(EndpointEntity endpointEntity)
        {
            return new EndpointDto
            {
                Selector = endpointEntity.Selector,
                Uri = endpointEntity.Uri,
                HttpVerb = endpointEntity.HttpVerb,
                Authentication = MapAuthentication(endpointEntity.Authentication)
            };
        }

        public UriTransformDto MapUriTransform(UriTransformEntity uriTransformEntity)
        {
            return uriTransformEntity == null ? null : new UriTransformDto { Replace = uriTransformEntity.Replace };
        }

        public AuthenticationDto MapAuthentication(AuthenticationEntity authenticationEntity)
        {
            return authenticationEntity switch
            {
                BasicAuthenticationEntity ent => new BasicAuthenticationDto
                {
                    Username = ent.Username,
                    PasswordKeyName = ent.PasswordKeyName
                },
                OidcAuthenticationEntity ent => new OidcAuthenticationDto
                {
                    Uri = ent.Uri,
                    ClientId = ent.ClientId,
                    Scopes = ent.Scopes?.ToList(),
                    ClientSecretKeyName = ent.ClientSecretKeyName,
                    UseHeaders = ent.UseHeaders,
                },
                null => new NoAuthenticationDto(),
                _ => null
            };
        }

    }
}
