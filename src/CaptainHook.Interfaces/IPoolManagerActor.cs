﻿namespace CaptainHook.Interfaces
{
    using System;
    using System.Threading.Tasks;
    using Common;
    using Microsoft.ServiceFabric.Actors;

    /// <summary>
    /// This interface defines the methods exposed by an actor.
    /// Clients use this interface to interact with the actor that implements it.
    /// </summary>
    public interface IPoolManagerActor : IActor
    {
        Task<Guid> DoWork(MessageData messageData);

        Task CompleteWork(Guid handle);
    }
}
