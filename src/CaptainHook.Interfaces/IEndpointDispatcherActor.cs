namespace CaptainHook.Interfaces
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Actors;

    /// <summary>
    /// This interface defines the methods exposed by an actor.
    /// Clients use this interface to interact with the actor that implements it.
    /// </summary>
    public interface IEndpointDispatcherActor : IActor
    {
        /// <summary>
        /// </summary>
        /// <returns></returns>
        Task<int> GetCountAsync(CancellationToken cancellationToken);

        /// <summary>
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        Task SetCountAsync(int count, CancellationToken cancellationToken);
    }
}
