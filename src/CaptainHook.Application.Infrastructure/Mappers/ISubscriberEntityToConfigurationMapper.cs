using System.Collections.Generic;
using System.Threading.Tasks;
using CaptainHook.Common.Configuration;
using CaptainHook.Domain.Entities;

namespace CaptainHook.Application.Infrastructure.Mappers
{
    public interface ISubscriberEntityToConfigurationMapper
    {
        Task<IEnumerable<SubscriberConfiguration>> MapSubscriber(SubscriberEntity cosmosModel);
    }
}