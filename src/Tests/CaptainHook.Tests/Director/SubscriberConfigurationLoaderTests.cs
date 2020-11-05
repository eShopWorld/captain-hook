using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CaptainHook.Application.Infrastructure.Mappers;
using CaptainHook.Common.Configuration;
using CaptainHook.DirectorService.Infrastructure;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Errors;
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
        private readonly Mock<ISubscriberEntityToConfigurationMapper> _mapperMock;
        private readonly Mock<ISubscriberRepository> _repositoryMock;

        private readonly SubscriberConfigurationLoader _subscriberConfigurationLoader;

        public SubscriberConfigurationLoaderTests()
        {
            _mapperMock = new Mock<ISubscriberEntityToConfigurationMapper>();
            _repositoryMock = new Mock<ISubscriberRepository>();

            _subscriberConfigurationLoader = new SubscriberConfigurationLoader(_repositoryMock.Object, _mapperMock.Object);
        }

        [Fact, IsDev]
        public async Task LoadAsync_TwoSubscriberEntities_MappedToTwoConfigurations()
        {
            // valid subscribers entities are needed to run the loop without failing mapper, but the actual data is not important for this test
            _repositoryMock.Setup(r => r.GetAllSubscribersAsync()).ReturnsAsync(new[] { new SubscriberEntity(string.Empty), new SubscriberEntity(string.Empty) });

            var firstConfig = new SubscriberConfigurationBuilder().WithType("event").WithSubscriberName("subscriber").WithCallback().Create();
            var secondConfig = new SubscriberConfigurationBuilder().WithType("other-event").WithSubscriberName("subscriber").Create();
            _mapperMock.SetupSequence(x => x.MapToWebhookAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(firstConfig)
                .ReturnsAsync(secondConfig);

            var result = await _subscriberConfigurationLoader.LoadAsync();

            result.Data.Should().BeEquivalentTo(new ReadOnlyCollection<SubscriberConfiguration>(new[] { firstConfig, secondConfig }));
            _repositoryMock.Verify(x => x.GetAllSubscribersAsync(), Times.Once);
            _mapperMock.Verify(x => x.MapToWebhookAsync(It.IsAny<SubscriberEntity>()), Times.Exactly(2));
            _mapperMock.Verify(x => x.MapToDlqAsync(It.IsAny<SubscriberEntity>()), Times.Never);
        }

        [Fact, IsDev]
        public async Task LoadAsync_SubscriberEntityWithDlq_MappedToTwoConfigurations()
        {
            // valid subscriber entity with DLQ hook is needed, but the actual data is not important for this test
            var subscriber = new SubscriberBuilder().WithDlqhook(string.Empty, string.Empty, string.Empty).Create();
            _repositoryMock.Setup(r => r.GetAllSubscribersAsync()).ReturnsAsync(new[] { subscriber });

            var firstConfig = new SubscriberConfigurationBuilder().WithType("event").WithSubscriberName("subscriber").Create();
            var secondConfig = new SubscriberConfigurationBuilder().WithType("event").WithSubscriberName("subscriber").CreateAsDlq();
            _mapperMock.Setup(x => x.MapToWebhookAsync(It.IsAny<SubscriberEntity>())).ReturnsAsync(firstConfig);
            _mapperMock.Setup(x => x.MapToDlqAsync(It.IsAny<SubscriberEntity>())).ReturnsAsync(secondConfig);

            var result = await _subscriberConfigurationLoader.LoadAsync();

            result.Data.Should().BeEquivalentTo(new ReadOnlyCollection<SubscriberConfiguration>(new[] { firstConfig, secondConfig }));
            _repositoryMock.Verify(x => x.GetAllSubscribersAsync(), Times.Once);
            _mapperMock.Verify(x => x.MapToWebhookAsync(It.IsAny<SubscriberEntity>()), Times.Once);
            _mapperMock.Verify(x => x.MapToDlqAsync(It.IsAny<SubscriberEntity>()), Times.Once);
        }

        [Fact, IsDev]
        public async Task LoadAsync_RepositoryReturnsError_SameErrorIsReturned()
        {
            _repositoryMock.Setup(r => r.GetAllSubscribersAsync()).ReturnsAsync(new MappingError("error"));

            var result = await _subscriberConfigurationLoader.LoadAsync();

            result.Error.Should().BeEquivalentTo(new MappingError("error"));
            _repositoryMock.Verify(x => x.GetAllSubscribersAsync(), Times.Once);
            _mapperMock.Verify(x => x.MapToWebhookAsync(It.IsAny<SubscriberEntity>()), Times.Never);
            _mapperMock.Verify(x => x.MapToDlqAsync(It.IsAny<SubscriberEntity>()), Times.Never);
        }

        [Fact, IsDev]
        public async Task LoadAsync_MapperReturnsErrorForOneWebhook_MappingErrorIsReturned()
        {
            _repositoryMock.Setup(r => r.GetAllSubscribersAsync())
                .ReturnsAsync(new[] { new SubscriberEntity(string.Empty), new SubscriberEntity(string.Empty), new SubscriberEntity(string.Empty) });
            _mapperMock.SetupSequence(x => x.MapToWebhookAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(new SubscriberConfigurationBuilder().Create())
                .ReturnsAsync(new MappingError("error"))
                .ReturnsAsync(new SubscriberConfigurationBuilder().Create());

            var result = await _subscriberConfigurationLoader.LoadAsync();

            result.Error.Should().BeEquivalentTo(new MappingError("Cannot map Cosmos DB entries"));
            _repositoryMock.Verify(x => x.GetAllSubscribersAsync(), Times.Once);
            _mapperMock.Verify(x => x.MapToWebhookAsync(It.IsAny<SubscriberEntity>()), Times.Exactly(3));
            _mapperMock.Verify(x => x.MapToDlqAsync(It.IsAny<SubscriberEntity>()), Times.Never);
        }

        [Fact, IsDev]
        public async Task LoadAsync_MapperThrowsExceptionForOneWebhook_MappingErrorWithExceptionFailureIsReturned()
        {
            _repositoryMock.Setup(r => r.GetAllSubscribersAsync())
                .ReturnsAsync(new[] { new SubscriberEntity(string.Empty), new SubscriberEntity(string.Empty), new SubscriberEntity(string.Empty) });
            _mapperMock.SetupSequence(x => x.MapToWebhookAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(new SubscriberConfigurationBuilder().Create())
                .ThrowsAsync(new Exception("error"))
                .ReturnsAsync(new SubscriberConfigurationBuilder().Create());

            var result = await _subscriberConfigurationLoader.LoadAsync();

            result.Error.Should().BeEquivalentTo(new MappingError("Cannot map Cosmos DB entries", new ExceptionFailure(new Exception("error"))));
            _repositoryMock.Verify(x => x.GetAllSubscribersAsync(), Times.Once);
            _mapperMock.Verify(x => x.MapToWebhookAsync(It.IsAny<SubscriberEntity>()), Times.Exactly(3));
            _mapperMock.Verify(x => x.MapToDlqAsync(It.IsAny<SubscriberEntity>()), Times.Never);
        }

        [Fact, IsDev]
        public async Task LoadAsync_MapperReturnsErrorForOneDlqhook_MappingErrorIsReturned()
        {
            _repositoryMock.Setup(r => r.GetAllSubscribersAsync())
                .ReturnsAsync(new[] {
                    new SubscriberBuilder().WithDlqhook(string.Empty, string.Empty, string.Empty).Create(),
                    new SubscriberBuilder().WithDlqhook(string.Empty, string.Empty, string.Empty).Create(),
                    new SubscriberBuilder().WithDlqhook(string.Empty, string.Empty, string.Empty).Create()
                });

            _mapperMock.Setup(x => x.MapToWebhookAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(new SubscriberConfigurationBuilder().Create());
            _mapperMock.SetupSequence(x => x.MapToDlqAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(new SubscriberConfigurationBuilder().CreateAsDlq())
                .ReturnsAsync(new MappingError("error"))
                .ReturnsAsync(new SubscriberConfigurationBuilder().CreateAsDlq());

            var result = await _subscriberConfigurationLoader.LoadAsync();

            result.Error.Should().BeEquivalentTo(new MappingError("Cannot map Cosmos DB entries"));
            _repositoryMock.Verify(x => x.GetAllSubscribersAsync(), Times.Once);
            _mapperMock.Verify(x => x.MapToWebhookAsync(It.IsAny<SubscriberEntity>()), Times.Exactly(3));
            _mapperMock.Verify(x => x.MapToDlqAsync(It.IsAny<SubscriberEntity>()), Times.Exactly(3));
        }

        [Fact, IsDev]
        public async Task LoadAsync_MapperThrowsExceptionForOneDlqhook_MappingErrorIsReturned()
        {
            _repositoryMock.Setup(r => r.GetAllSubscribersAsync())
                .ReturnsAsync(new[] {
                    new SubscriberBuilder().WithDlqhook(string.Empty, string.Empty, string.Empty).Create(),
                    new SubscriberBuilder().WithDlqhook(string.Empty, string.Empty, string.Empty).Create(),
                    new SubscriberBuilder().WithDlqhook(string.Empty, string.Empty, string.Empty).Create()
                });

            _mapperMock.Setup(x => x.MapToWebhookAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(new SubscriberConfigurationBuilder().Create());
            _mapperMock.SetupSequence(x => x.MapToDlqAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(new SubscriberConfigurationBuilder().CreateAsDlq())
                .ThrowsAsync(new Exception("error"))
                .ReturnsAsync(new SubscriberConfigurationBuilder().CreateAsDlq());

            var result = await _subscriberConfigurationLoader.LoadAsync();

            result.Error.Should().BeEquivalentTo(new MappingError("Cannot map Cosmos DB entries", new ExceptionFailure(new Exception("error"))));
            _repositoryMock.Verify(x => x.GetAllSubscribersAsync(), Times.Once);
            _mapperMock.Verify(x => x.MapToWebhookAsync(It.IsAny<SubscriberEntity>()), Times.Exactly(3));
            _mapperMock.Verify(x => x.MapToDlqAsync(It.IsAny<SubscriberEntity>()), Times.Exactly(3));
        }
    }
}
