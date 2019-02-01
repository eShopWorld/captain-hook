﻿namespace CaptainHook.EventHandlerActor.Handlers
{
    using System.Threading.Tasks;
    using Common.Nasty;

    public interface IHandler
    {
        Task Call<TRequest>(TRequest request);

        Task<DispatchHttpResponse> Call<TRequest, TResponse>(TRequest request);
    }
}