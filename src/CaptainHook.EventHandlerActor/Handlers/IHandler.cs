namespace CaptainHook.EventHandlerActor.Handlers
{
    using System.Threading.Tasks;

    public interface IHandler
    {
        Task Call<TRequest>(TRequest request);

        Task<TResponse> Call<TRequest, TResponse>(TRequest request);
    }
}