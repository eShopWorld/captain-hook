﻿using CaptainHook.Common.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CaptainHook.DirectorService.Infrastructure.Interfaces
{
    public interface ISubscriberConfigurationLoader
    {
        Task<(IList<WebhookConfig>, IList<SubscriberConfiguration>)> LoadAsync();
    }
}