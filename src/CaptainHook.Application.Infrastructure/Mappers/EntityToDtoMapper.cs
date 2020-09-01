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
                Webhooks = new WebhooksDto
                {
                    SelectionRule = subscriberEntity.Webhooks.SelectionRule,
                    UriTransform = MapUriTransform(subscriberEntity.Webhooks.UriTransform),
                    Endpoints = subscriberEntity.Webhooks.Endpoints.Select(MapEndpoint).ToList()
                }
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
                    ClientSecretKeyName = ent.ClientSecretKeyName
                },
                _ => null
            };
        }

    }
}
