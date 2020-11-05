using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CaptainHook.Application.Infrastructure.Mappers;
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
        //private readonly Mock<ISubscribersKeyVaultProvider> _subscriberKeyVaultProviderMock;
        private readonly Mock<ISubscriberEntityToConfigurationMapper> _entityToConfigurationMapperMock;
        private readonly Mock<ISubscriberRepository> _subscriberRepositoryMock;
        //private readonly Mock<IConfigurationMerger> _configurationMergerMock;

        private readonly SubscriberConfigurationLoader _subscriberConfigurationLoader;

        public SubscriberConfigurationLoaderTests()
        {
            //_subscriberKeyVaultProviderMock = new Mock<ISubscribersKeyVaultProvider>();
            _entityToConfigurationMapperMock = new Mock<ISubscriberEntityToConfigurationMapper>();
            //_entityToConfigurationMapperMock.Setup(x => x.MapToWebhookAsync(It.IsAny<SubscriberEntity>())).ReturnsAsync(new SubscriberConfiguration());
            //_entityToConfigurationMapperMock.Setup(x => x.MapToDlqAsync(It.IsAny<SubscriberEntity>())).ReturnsAsync(new SubscriberConfiguration());
            _subscriberRepositoryMock = new Mock<ISubscriberRepository>();
            //_configurationMergerMock = new Mock<IConfigurationMerger>();

            _subscriberConfigurationLoader = new SubscriberConfigurationLoader(_subscriberRepositoryMock.Object, _entityToConfigurationMapperMock.Object);
        }

        [Fact, IsDev]
        public async Task LoadAsync_TwoSubscriberEntities_MappedToTwoConfigurations()
        {
            
            //var firstEntity = new SubscriberBuilder().WithEvent("event").WithName("subscriber").Create();
            //var secondEntity = new SubscriberBuilder().WithEvent("other-event").WithName("subscriber").Create();
            //var subscriberEntities = new[] { firstEntity, secondEntity };
            
            
            // valid subscribers entities are needed to run the loop and to not fail mapper
            _subscriberRepositoryMock.Setup(r => r.GetAllSubscribersAsync()).ReturnsAsync(new[] { new SubscriberEntity(""), new SubscriberEntity("") });

            var firstConfig = new SubscriberConfigurationBuilder().WithType("event").WithSubscriberName("subscriber").Create();
            var secondConfig = new SubscriberConfigurationBuilder().WithType("other-event").WithSubscriberName("subscriber").Create();
            //var subscriberConfigurations = new List<SubscriberConfiguration> { firstConfig, secondConfig, };

            _entityToConfigurationMapperMock.SetupSequence(x => x.MapToWebhookAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(firstConfig)
                .ReturnsAsync(secondConfig);

            //_entityToConfigurationMapperMock.Setup(x => x.MapToDlqAsync(It.IsAny<SubscriberEntity>())).ReturnsAsync(secondConfig);

            //_subscriberKeyVaultProviderMock.Setup(x => x.Load(It.IsAny<string>())).Returns(subscriberConfigurations);

            var firstResultConfig = new SubscriberConfigurationBuilder().WithType("event").WithSubscriberName("subscriber").Create();
            var secondResultConfig = new SubscriberConfigurationBuilder().WithType("other-event").WithSubscriberName("subscriber").Create();
            var expectedResult = new OperationResult<ReadOnlyCollection<SubscriberConfiguration>>(
                new ReadOnlyCollection<SubscriberConfiguration>(new[] { firstResultConfig, secondResultConfig }));
            //_configurationMergerMock.Setup(x => x.MergeAsync(subscriberConfigurations, subscriberEntities))
            //    .ReturnsAsync(expectedResult);

            var result = await _subscriberConfigurationLoader.LoadAsync();

            result.Should().BeEquivalentTo(expectedResult);
            _subscriberRepositoryMock.Verify(x => x.GetAllSubscribersAsync(), Times.Once);
            //_subscriberKeyVaultProviderMock.Verify(x => x.Load(It.IsAny<string>()), Times.Once);
            //_configurationMergerMock.Verify(x => x.MergeAsync(It.IsAny<IEnumerable<SubscriberConfiguration>>(), It.IsAny<IEnumerable<SubscriberEntity>>()), Times.Once);
        }
    }
}
