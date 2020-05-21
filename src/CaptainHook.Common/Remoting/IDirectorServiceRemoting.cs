﻿using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;

namespace CaptainHook.Common.Remoting
{
    public interface IDirectorServiceRemoting: IService
    {
        Task<int> GetConfigurationForEventAsync(string eventName);

        Task ReloadConfigurationForEventAsync(string eventName);
    }
}