using CaptainHook.Contract;
using CaptainHook.Domain.Entities;
using System.Linq;

namespace CaptainHook.Application.Infrastructure.Mappers
{
    public class DtoToEntityMapper : IDtoToEntityMapper
    {
        public WebhooksEntity MapWebooks(WebhooksDto webhooksDto)
        {
            return new WebhooksEntity(
                webhooksDto.SelectionRule,
                webhooksDto.Endpoints?.Select(endpointDto => MapEndpoint(endpointDto)) ?? Enumerable.Empty<EndpointEntity>(),
                MapUriTransform(webhooksDto.UriTransform));
        }

        public EndpointEntity MapEndpoint(EndpointDto endpointDto, string selector = null)
        {
            var authenticationEntity = MapAuthentication(endpointDto.Authentication);
            var endpoint = new EndpointEntity(endpointDto.Uri, authenticationEntity, endpointDto.HttpVerb, selector ?? endpointDto.Selector);

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

        public AuthenticationEntity MapAuthentication(AuthenticationDto authenticationDto)
        {
            return authenticationDto switch
            {
                BasicAuthenticationDto dto => new BasicAuthenticationEntity(dto.Username, dto.Password),
                OidcAuthenticationDto dto => new OidcAuthenticationEntity(dto.ClientId, dto.ClientSecretKeyName, dto.Uri, dto.Scopes?.ToArray()),
                _ => null,
            };
        }
    }
}
