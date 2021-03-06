﻿using System;
using System.Threading.Tasks;
using CaptainHook.Application.Infrastructure.Mappers;
using CaptainHook.DirectorService.Infrastructure;
using CaptainHook.DirectorService.Infrastructure.Interfaces;
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

        [Fact, IsUnit]
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

            result.Should().BeEquivalentTo(new LoadingSubscribersResult(firstConfig, secondConfig));
            _repositoryMock.Verify(x => x.GetAllSubscribersAsync(), Times.Once);
            _mapperMock.Verify(x => x.MapToWebhookAsync(It.IsAny<SubscriberEntity>()), Times.Exactly(2));
            _mapperMock.Verify(x => x.MapToDlqAsync(It.IsAny<SubscriberEntity>()), Times.Never);
        }

        [Fact, IsUnit]
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

            result.Should().BeEquivalentTo(new LoadingSubscribersResult(firstConfig, secondConfig));
            _repositoryMock.Verify(x => x.GetAllSubscribersAsync(), Times.Once);
            _mapperMock.Verify(x => x.MapToWebhookAsync(It.IsAny<SubscriberEntity>()), Times.Once);
            _mapperMock.Verify(x => x.MapToDlqAsync(It.IsAny<SubscriberEntity>()), Times.Once);
        }

        [Fact, IsUnit]
        public async Task LoadAsync_RepositoryReturnsError_SameErrorIsReturned()
        {
            _repositoryMock.Setup(r => r.GetAllSubscribersAsync()).ReturnsAsync(new MappingError("error"));

            var result = await _subscriberConfigurationLoader.LoadAsync();

            result.Should().BeEquivalentTo(new LoadingSubscribersResult(new MappingError("error")));
            _repositoryMock.Verify(x => x.GetAllSubscribersAsync(), Times.Once);
            _mapperMock.Verify(x => x.MapToWebhookAsync(It.IsAny<SubscriberEntity>()), Times.Never);
            _mapperMock.Verify(x => x.MapToDlqAsync(It.IsAny<SubscriberEntity>()), Times.Never);
        }

        [Fact, IsUnit]
        public async Task LoadAsync_MapperReturnsErrorForSomeWebhooks_ErrorsAreReturned()
        {
            var subscriberConfiguration = new SubscriberConfigurationBuilder().Create();
            _repositoryMock.Setup(r => r.GetAllSubscribersAsync())
                .ReturnsAsync(new[] {
                    new SubscriberEntity(string.Empty),
                    new SubscriberEntity("sub1", new EventEntity("event1")),
                    new SubscriberEntity("sub2", new EventEntity("event2")),
                    new SubscriberEntity(string.Empty)
                });
            _mapperMock.SetupSequence(x => x.MapToWebhookAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(subscriberConfiguration)
                .ReturnsAsync(new BusinessError("error1"))
                .ReturnsAsync(new BusinessError("error2"))
                .ReturnsAsync(subscriberConfiguration);

            var result = await _subscriberConfigurationLoader.LoadAsync();

            var expected = new LoadingSubscribersResult(
                new[] { subscriberConfiguration, subscriberConfiguration },
                new[] { new BusinessError("error1"), new BusinessError("error2") });
            result.Should().BeEquivalentTo(expected);
            _repositoryMock.Verify(x => x.GetAllSubscribersAsync(), Times.Once);
            _mapperMock.Verify(x => x.MapToWebhookAsync(It.IsAny<SubscriberEntity>()), Times.Exactly(4));
            _mapperMock.Verify(x => x.MapToDlqAsync(It.IsAny<SubscriberEntity>()), Times.Never);
        }

        [Fact, IsUnit]
        public async Task LoadAsync_MapperThrowsExceptionForSomeWebhooks_MappingErrorsWithExceptionFailuresAreReturned()
        {
            var subscriberConfiguration = new SubscriberConfigurationBuilder().Create();
            _repositoryMock.Setup(r => r.GetAllSubscribersAsync())
                .ReturnsAsync(new[] {
                    new SubscriberEntity(string.Empty),
                    new SubscriberEntity("sub1", new EventEntity("event1")),
                    new SubscriberEntity("sub2", new EventEntity("event2")),
                    new SubscriberEntity(string.Empty)
                });
            _mapperMock.SetupSequence(x => x.MapToWebhookAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(subscriberConfiguration)
                .ThrowsAsync(new Exception("error1"))
                .ThrowsAsync(new Exception("error2"))
                .ReturnsAsync(subscriberConfiguration);

            var result = await _subscriberConfigurationLoader.LoadAsync();

            var expected = new LoadingSubscribersResult(
               new[] { subscriberConfiguration, subscriberConfiguration },
               new[] {
                   new MappingError("Cannot map SubscriberEntity to SubscriberConfiguration. SubscriberId: event1-sub1", new ExceptionFailure(new Exception("error1"))),
                   new MappingError("Cannot map SubscriberEntity to SubscriberConfiguration. SubscriberId: event2-sub2", new ExceptionFailure(new Exception("error2")))
               });
            result.Should().BeEquivalentTo(expected);
            _repositoryMock.Verify(x => x.GetAllSubscribersAsync(), Times.Once);
            _mapperMock.Verify(x => x.MapToWebhookAsync(It.IsAny<SubscriberEntity>()), Times.Exactly(4));
            _mapperMock.Verify(x => x.MapToDlqAsync(It.IsAny<SubscriberEntity>()), Times.Never);
        }

        [Fact, IsUnit]
        public async Task LoadAsync_MapperReturnsErrorsForSomeDlqhooks_MappingErrorIsReturned()
        {
            var subscriberConfiguration = new SubscriberConfigurationBuilder().Create();
            var dlqSubscriberConfiguration = new SubscriberConfigurationBuilder().CreateAsDlq();
            _repositoryMock.Setup(r => r.GetAllSubscribersAsync())
                .ReturnsAsync(new[] {
                    new SubscriberBuilder().WithDlqhook(string.Empty, string.Empty, string.Empty).Create(),
                    new SubscriberBuilder().WithName("sub1").WithEvent("event1").WithDlqhook(string.Empty, string.Empty, string.Empty).Create(),
                    new SubscriberBuilder().WithName("sub2").WithEvent("event2").WithDlqhook(string.Empty, string.Empty, string.Empty).Create(),
                    new SubscriberBuilder().WithDlqhook(string.Empty, string.Empty, string.Empty).Create()
                });
            _mapperMock.Setup(x => x.MapToWebhookAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(subscriberConfiguration);
            _mapperMock.SetupSequence(x => x.MapToDlqAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(dlqSubscriberConfiguration)
                .ReturnsAsync(new BusinessError("error1"))
                .ReturnsAsync(new BusinessError("error2"))
                .ReturnsAsync(dlqSubscriberConfiguration);

            var result = await _subscriberConfigurationLoader.LoadAsync();

            var expected = new LoadingSubscribersResult(
                new[] { subscriberConfiguration, dlqSubscriberConfiguration, subscriberConfiguration, subscriberConfiguration, subscriberConfiguration, dlqSubscriberConfiguration },
                new[] { new BusinessError("error1"), new BusinessError("error2") });
            result.Should().BeEquivalentTo(expected);
            _repositoryMock.Verify(x => x.GetAllSubscribersAsync(), Times.Once);
            _mapperMock.Verify(x => x.MapToWebhookAsync(It.IsAny<SubscriberEntity>()), Times.Exactly(4));
            _mapperMock.Verify(x => x.MapToDlqAsync(It.IsAny<SubscriberEntity>()), Times.Exactly(4));
        }

        [Fact, IsUnit]
        public async Task LoadAsync_MapperThrowsExceptionForOneDlqhook_MappingErrorIsReturned()
        {
            var subscriberConfiguration = new SubscriberConfigurationBuilder().Create();
            var dlqSubscriberConfiguration = new SubscriberConfigurationBuilder().CreateAsDlq();
            _repositoryMock.Setup(r => r.GetAllSubscribersAsync())
                .ReturnsAsync(new[] {
                    new SubscriberBuilder().WithDlqhook(string.Empty, string.Empty, string.Empty).Create(),
                    new SubscriberBuilder().WithName("sub1").WithEvent("event1").WithDlqhook(string.Empty, string.Empty, string.Empty).Create(),
                    new SubscriberBuilder().WithName("sub2").WithEvent("event2").WithDlqhook(string.Empty, string.Empty, string.Empty).Create(),
                    new SubscriberBuilder().WithDlqhook(string.Empty, string.Empty, string.Empty).Create()
                });
            _mapperMock.Setup(x => x.MapToWebhookAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(subscriberConfiguration);
            _mapperMock.SetupSequence(x => x.MapToDlqAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(dlqSubscriberConfiguration)
                .ThrowsAsync(new Exception("error1"))
                .ThrowsAsync(new Exception("error2"))
                .ReturnsAsync(dlqSubscriberConfiguration);

            var result = await _subscriberConfigurationLoader.LoadAsync();

            var expected = new LoadingSubscribersResult(
               new[] { subscriberConfiguration, dlqSubscriberConfiguration, subscriberConfiguration, subscriberConfiguration, subscriberConfiguration, dlqSubscriberConfiguration },
               new[]
               {
                   new MappingError("Cannot map SubscriberEntity to SubscriberConfiguration. SubscriberId: event1-sub1", new ExceptionFailure(new Exception("error1"))),
                   new MappingError("Cannot map SubscriberEntity to SubscriberConfiguration. SubscriberId: event2-sub2", new ExceptionFailure(new Exception("error2")))
               });
            result.Should().BeEquivalentTo(expected);
            _repositoryMock.Verify(x => x.GetAllSubscribersAsync(), Times.Once);
            _mapperMock.Verify(x => x.MapToWebhookAsync(It.IsAny<SubscriberEntity>()), Times.Exactly(4));
            _mapperMock.Verify(x => x.MapToDlqAsync(It.IsAny<SubscriberEntity>()), Times.Exactly(4));
        }
    }
}
