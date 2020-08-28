using CaptainHook.Contract;
using CaptainHook.Domain.Entities;

namespace CaptainHook.Application.Infrastructure.Mappers
{
    public interface IEntityToDtoMapper
    {
        /// <summary>
        /// Maps a Subscriber entity to a Subscriber DTO
        /// </summary>
        /// <param name="subscriberEntity">A Subscriber entity</param>
        /// <returns>A Subscriber DTO</returns>
        SubscriberDto MapSubscriber(SubscriberEntity subscriberEntity);

        /// <summary>
        /// Maps an Endpoint entity to an Endpoint DTO
        /// </summary>
        /// <param name="endpointEntity">An Endpoint entity</param>
        /// <returns>An Endpoint DTO</returns>
        EndpointDto MapEndpoint(EndpointEntity endpointEntity);

        /// <summary>
        /// Maps a URITransform entity to a URITransform DTO
        /// </summary>
        /// <param name="uriTransformEntity">A URITransform entity</param>
        /// <returns>A URITransform DTO</returns>
        UriTransformDto MapUriTransform(UriTransformEntity uriTransformEntity);

        /// <summary>
        /// Maps an Authentication entity to an Authentication DTO
        /// </summary>
        /// <param name="authenticationEntity">An Authentication entity</param>
        /// <returns>An Authentication DTO</returns>
        AuthenticationDto MapAuthentication(AuthenticationEntity authenticationEntity);
    }
}
