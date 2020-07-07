using CaptainHook.Common.Configuration;
using CaptainHook.DirectorService.Infrastructure;
using CaptainHook.DirectorService.Infrastructure.Interfaces;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Repositories;
using Eshopworld.Tests.Core;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace CaptainHook.Tests.Director
{
    public class SubscriberConfigurationLoaderTests
    {
        private readonly Mock<ISubscriberRepository> _subscriberRepositoryMock;
        private readonly Mock<IConfigurationMerger> _configurationMergerMock;
        
        private readonly SubscriberConfigurationLoader _subscriberConfigurationLoader;

        public SubscriberConfigurationLoaderTests()
        {
            _subscriberRepositoryMock = new Mock<ISubscriberRepository>();
            _configurationMergerMock = new Mock<IConfigurationMerger>();

            _subscriberConfigurationLoader = new SubscriberConfigurationLoader(_subscriberRepositoryMock.Object, _configurationMergerMock.Object);
        }

        [Fact, IsDev]
        public async Task MergeIsInvoked()
        {
            var result = await _subscriberConfigurationLoader.LoadAsync("https://esw-tooling-ci-we.vault.azure.net/");
            _configurationMergerMock.Verify(x => x.MergeAsync(It.IsAny<IEnumerable<SubscriberConfiguration>>(), It.IsAny<IEnumerable<SubscriberEntity>>()));
        }
    }
}
