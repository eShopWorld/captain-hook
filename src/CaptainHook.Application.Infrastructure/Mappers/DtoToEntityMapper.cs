using CaptainHook.Contract;
using CaptainHook.Domain.Entities;
using System.Linq;

namespace CaptainHook.Application.Infrastructure.Mappers
{
    public class DtoToEntityMapper : IDtoToEntityMapper
    {
        public WebhooksEntity MapWebooks(WebhooksDto webhooksDto, WebhooksEntityType type)
        {
            return new WebhooksEntity(
                type,
                webhooksDto.SelectionRule,
                webhooksDto.Endpoints?.Select(endpointDto => MapEndpoint(endpointDto)) ?? Enumerable.Empty<EndpointEntity>(),
                MapUriTransform(webhooksDto.UriTransform),
                MapPayloadTransform(webhooksDto.PayloadTransform, type));
        }

        public EndpointEntity MapEndpoint(EndpointDto endpointDto, string selector = null)
        {
            var authenticationEntity = MapAuthentication(endpointDto.Authentication);
            var endpoint = new EndpointEntity(
                endpointDto.Uri,
                authenticationEntity,
                endpointDto.HttpVerb,
                selector ?? endpointDto.Selector,
                endpointDto.RetrySleepDurations,
                endpointDto.Timeout);

            return endpoint;
        }

        public UriTransformEntity MapUriTransform(UriTransformDto uriTransformDto)
        {
            if (uriTransformDto?.Replace == null)
            {
                return null;
            }

            return new UriTransformEntity(uriTransformDto.Replace);
        }

        public string MapPayloadTransform(string payloadTransform, WebhooksEntityType type)
        {
            if (type == WebhooksEntityType.Callbacks)
                return null;

            return payloadTransform?.ToLowerInvariant() switch
            {
                "request" => "$.Request",
                "response" => "$.Response",
                "orderconfirmation" => "$.OrderConfirmation",
                "platformorderconfirmation" => "$.PlatformOrderConfirmation",
                "emptycart" => "$.EmptyCart",
                _ => "$"
            };
        }

        public AuthenticationEntity MapAuthentication(AuthenticationDto authenticationDto)
        {
            return authenticationDto switch
            {
                BasicAuthenticationDto dto => new BasicAuthenticationEntity(dto.Username, dto.PasswordKeyName),
                OidcAuthenticationDto dto => new OidcAuthenticationEntity(dto.ClientId, dto.ClientSecretKeyName, dto.Uri, dto.Scopes?.ToArray(), dto.UseHeaders),
                NoAuthenticationDto _ => null,
                _ => null,
            };
        }
    }
}
