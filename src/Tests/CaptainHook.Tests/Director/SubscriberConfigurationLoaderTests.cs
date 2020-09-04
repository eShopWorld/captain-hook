using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CaptainHook.Common.Configuration;
using CaptainHook.DirectorService.Infrastructure;
using CaptainHook.DirectorService.Infrastructure.Interfaces;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Repositories;
using CaptainHook.Domain.Results;
using CaptainHook.TestsInfrastructure.Builders;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Moq;
using Xunit;

namespace CaptainHook.Tests.Director
{
    public class SubscriberConfigurationLoaderTests
    {
        private readonly Mock<ISubscribersKeyVaultProvider> _subscriberKeyVaultProviderMock;
        private readonly Mock<ISubscriberRepository> _subscriberRepositoryMock;
        private readonly Mock<IConfigurationMerger> _configurationMergerMock;

        private readonly SubscriberConfigurationLoader _subscriberConfigurationLoader;

        public SubscriberConfigurationLoaderTests()
        {
            _subscriberKeyVaultProviderMock = new Mock<ISubscribersKeyVaultProvider>();
            _subscriberRepositoryMock = new Mock<ISubscriberRepository>();
            _configurationMergerMock = new Mock<IConfigurationMerger>();

            _subscriberConfigurationLoader = new SubscriberConfigurationLoader(_subscriberRepositoryMock.Object, _configurationMergerMock.Object, _subscriberKeyVaultProviderMock.Object);
        }

        [Fact, IsDev]
        public async Task MergeIsInvoked()
        {
            var firstEntity = new SubscriberBuilder().WithEvent("event").WithName("entity-subscriber").Create();
            var secondEntity = new SubscriberBuilder().WithEvent("second-event").WithName("entity-subscriber").Create();
            var subscriberEntities = new[] { firstEntity, secondEntity };

            _subscriberRepositoryMock.Setup(r => r.GetAllSubscribersAsync()).ReturnsAsync(subscriberEntities);

            var firstConfig = new SubscriberConfigurationBuilder().WithType("event").WithSubscriberName("subscriber").Create();
            var secondConfig = new SubscriberConfigurationBuilder().WithType("other-event").WithSubscriberName("subscriber").Create();
            var dictionary = new Dictionary<string, SubscriberConfiguration>
            {
                [SubscriberConfiguration.Key(firstConfig.EventType, firstConfig.SubscriberName)] = firstConfig,
                [SubscriberConfiguration.Key(secondConfig.EventType, secondConfig.SubscriberName)] = secondConfig,
            };
            _subscriberKeyVaultProviderMock.Setup(x => x.Load(It.IsAny<string>())).Returns(dictionary);

            var firstResultConfig = new SubscriberConfigurationBuilder().WithType("event1").WithSubscriberName("subscriber").Create();
            var secondResultConfig = new SubscriberConfigurationBuilder().WithType("other-event1").WithSubscriberName("subscriber").Create();
            var expectedResult = new OperationResult<ReadOnlyCollection<SubscriberConfiguration>>(
                new ReadOnlyCollection<SubscriberConfiguration>(new[] { firstResultConfig, secondResultConfig }));
            _configurationMergerMock.Setup(x => x.MergeAsync(dictionary.Values, subscriberEntities))
                .ReturnsAsync(expectedResult);

            var result = await _subscriberConfigurationLoader.LoadAsync("key-vault");

            result.Should().BeEquivalentTo(expectedResult);
            _subscriberRepositoryMock.Verify(x => x.GetAllSubscribersAsync(), Times.Once);
            _subscriberKeyVaultProviderMock.Verify(x => x.Load(It.IsAny<string>()), Times.Once);
            _configurationMergerMock.Verify(x => x.MergeAsync(It.IsAny<IEnumerable<SubscriberConfiguration>>(), It.IsAny<IEnumerable<SubscriberEntity>>()), Times.Once);
        }
    }
}
