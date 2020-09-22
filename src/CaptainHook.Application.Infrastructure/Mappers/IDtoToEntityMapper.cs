using CaptainHook.Contract;
using CaptainHook.Domain.Entities;

namespace CaptainHook.Application.Infrastructure.Mappers
{
    public interface IDtoToEntityMapper
    {
        /// <summary>
        /// Maps a Webooks DTO to a Webooks entity
        /// </summary>
        /// <param name="webhooksDto">A Webooks DTO</param>
        /// <param name="type">The entity type</param>
        /// <returns>A Webooks entity</returns>
        WebhooksEntity MapWebooks(WebhooksDto webhooksDto, WebhooksEntityType type);

        /// <summary>
        /// Maps an Endpoint DTO to an Endpoint entity
        /// </summary>
        /// <param name="endpointDto">An Endpoint DTO</param>
        /// <param name="selector"></param>
        /// <returns>An Endpoint entity</returns>
        EndpointEntity MapEndpoint(EndpointDto endpointDto, string selector = null);

        /// <summary>
        /// Maps a URITransfrom DTO to a URITransform entity
        /// </summary>
        /// <param name="uriTransformDto">A URI Transfor DTO</param>
        /// <returns>A URI Transfor entity</returns>
        UriTransformEntity MapUriTransform(UriTransformDto uriTransformDto);

        /// <summary>
        /// Maps an Authentication DTO to an Authentication entity
        /// </summary>
        /// <param name="authenticationDto">An Authentication DTO</param>
        /// <returns>An Authentication entity</returns>
        AuthenticationEntity MapAuthentication(AuthenticationDto authenticationDto);
    }
}
