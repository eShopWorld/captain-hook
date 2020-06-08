﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common.Configuration;
using CaptainHook.DirectorService;
using CaptainHook.DirectorService.Events;
using CaptainHook.DirectorService.Infrastructure;
using CaptainHook.DirectorService.Infrastructure.Interfaces;
using CaptainHook.Tests.Builders;
using Eshopworld.Core;
using Eshopworld.Tests.Core;
using Moq;
using Xunit;

namespace CaptainHook.Tests.Director
{
    public class ReaderServicesManagerTests
    {
        private readonly SubscriberConfiguration[] _subscribersToCreate =
        {
            new SubscriberConfigurationBuilder().WithType("testevent").WithCallback().Create(),
            new SubscriberConfigurationBuilder().WithType("testevent").WithSubscriberName("subscriber1").Create(),
            new SubscriberConfigurationBuilder().WithType("testevent.completed").Create(),
        };

        private readonly WebhookConfig[] _webhooks =
        {
            new WebhookConfig { Name = "testevent" },
            new WebhookConfig { Name = "testevent.completed" }
        };

        private readonly Mock<IBigBrother> _bigBrotherMock = new Mock<IBigBrother>();
        private readonly Mock<IFabricClientWrapper> _fabricClientMock = new Mock<IFabricClientWrapper>();
        private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new Mock<IDateTimeProvider>();

        private ReaderServicesManager CreateReaderServiceManager()
        {
            _dateTimeProviderMock.SetupGet(x => x.UtcNow)
                .Returns(DateTimeOffset.MinValue.AddMilliseconds(10000000000000));
            var generator = new ReaderServiceNameGenerator(_dateTimeProviderMock.Object);
            return new ReaderServicesManager(_fabricClientMock.Object, _bigBrotherMock.Object, _dateTimeProviderMock.Object, generator);
        }

        private void ProgressTime(int milliseconds)
        {
            _dateTimeProviderMock.SetupGet(x => x.UtcNow).Returns(DateTimeOffset.MinValue.AddMilliseconds(10000000000000 + milliseconds));
        }

        [Fact, IsLayer0]
        public async Task CreateReadersAsync_ForFreshEnvironment_ShouldCallFabricClientWrapper()
        {
            var readerServiceManager = CreateReaderServiceManager();

            var deployedServicesNames = Enumerable.Empty<string>();

            await readerServiceManager.CreateReadersAsync(_subscribersToCreate, deployedServicesNames, _webhooks, CancellationToken.None);

            VerifyFabricClientCreateCalls("fabric:/CaptainHook/EventReader.testevent-captain-hook-10000000000000",
                "fabric:/CaptainHook/EventReader.testevent-subscriber1-10000000000000",
                "fabric:/CaptainHook/EventReader.testevent.completed-captain-hook-10000000000000");

            VerifyFabricClientDeleteCalls();
        }

        [Fact, IsLayer0]
        public async Task CreateReadersAsync_ForExistingEnvironment_ShouldCallFabricClientWrapper()
        {
            var readerServiceManager = CreateReaderServiceManager();

            var deployedServicesNames = new[]
            {
                "fabric:/CaptainHook/EventReader.oldtestevent.completed-captain-hook-10000000000000",
                "fabric:/CaptainHook/EventReader.oldtestevent.completed-oldsubscriber-10000000000000",
                "fabric:/CaptainHook/EventReader.testevent-captain-hook-10000000000000",
                "fabric:/CaptainHook/EventReader.testevent-subscriber1-10000000000001",
                "fabric:/CaptainHook/EventReader.testevent.completed-captain-hook-10000000000000",
            };

            ProgressTime(2);

            await readerServiceManager.CreateReadersAsync(_subscribersToCreate, deployedServicesNames, _webhooks, CancellationToken.None);

            VerifyFabricClientCreateCalls("fabric:/CaptainHook/EventReader.testevent-captain-hook-10000000000002",
                "fabric:/CaptainHook/EventReader.testevent-subscriber1-10000000000002",
                "fabric:/CaptainHook/EventReader.testevent.completed-captain-hook-10000000000002");

            VerifyFabricClientDeleteCalls("fabric:/CaptainHook/EventReader.testevent-captain-hook-10000000000000",
                "fabric:/CaptainHook/EventReader.testevent-subscriber1-10000000000001",
                "fabric:/CaptainHook/EventReader.testevent.completed-captain-hook-10000000000000");
        }

        private void VerifyFabricClientCreateCalls(params string[] serviceNames)
        {
            _fabricClientMock.Verify(c => c.CreateServiceAsync(It.IsAny<ServiceCreationDescription>(), It.IsAny<CancellationToken>()), Times.Exactly(serviceNames.Length));

            foreach (var serviceName in serviceNames)
            {
                _fabricClientMock.Verify(c => c.CreateServiceAsync(It.Is<ServiceCreationDescription>(m => m.ServiceName == serviceName), It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        private void VerifyFabricClientDeleteCalls(params string[] serviceNames)
        {
            _fabricClientMock.Verify(c => c.DeleteServiceAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(serviceNames.Length));

            foreach (var serviceName in serviceNames)
            {
                _fabricClientMock.Verify(c => c.DeleteServiceAsync(It.Is<string>(m => m == serviceName), It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        [Fact, IsLayer0]
        public async Task CreateReadersAsync_ForFreshEnvironment_ShouldPublishTelemetryEvents()
        {
            var readerServiceManager = CreateReaderServiceManager();

            var deployedServicesNames = Enumerable.Empty<string>();

            await readerServiceManager.CreateReadersAsync(_subscribersToCreate, deployedServicesNames, _webhooks, CancellationToken.None);

            VerifyServiceCreatedEventPublished("fabric:/CaptainHook/EventReader.testevent-captain-hook-10000000000000",
                "fabric:/CaptainHook/EventReader.testevent-subscriber1-10000000000000",
                "fabric:/CaptainHook/EventReader.testevent.completed-captain-hook-10000000000000");

            VerifyServiceDeletedEventPublished();
        }

        [Fact, IsLayer0]
        public async Task CreateReadersAsync_ForExistingEnvironment_ShouldPublishTelemetryEvents()
        {
            var readerServiceManager = CreateReaderServiceManager();

            var deployedServicesNames = new[]
            {
                "fabric:/CaptainHook/EventReader.oldtestevent.completed-captain-hook-10000000000000",
                "fabric:/CaptainHook/EventReader.oldtestevent.completed-oldsubscriber-10000000000000",
                "fabric:/CaptainHook/EventReader.testevent-captain-hook-10000000000000",
                "fabric:/CaptainHook/EventReader.testevent-subscriber1-10000000000001",
                "fabric:/CaptainHook/EventReader.testevent.completed-captain-hook-10000000000000",
            };

            ProgressTime(2);

            await readerServiceManager.CreateReadersAsync(_subscribersToCreate, deployedServicesNames, _webhooks, CancellationToken.None);

            VerifyServiceCreatedEventPublished("fabric:/CaptainHook/EventReader.testevent-captain-hook-10000000000002",
                "fabric:/CaptainHook/EventReader.testevent-subscriber1-10000000000002",
                "fabric:/CaptainHook/EventReader.testevent.completed-captain-hook-10000000000002");

            VerifyServiceDeletedEventPublished("fabric:/CaptainHook/EventReader.testevent-captain-hook-10000000000000",
                "fabric:/CaptainHook/EventReader.testevent-subscriber1-10000000000001",
                "fabric:/CaptainHook/EventReader.testevent.completed-captain-hook-10000000000000");
        }

        private void VerifyServiceCreatedEventPublished(params string[] serviceNames)
        {
            _bigBrotherMock.Verify(b => b.Publish(It.IsAny<ServiceCreatedEvent>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Exactly(serviceNames.Length));

            foreach (var serviceName in serviceNames)
            {
                _bigBrotherMock.Verify(b => b.Publish(It.Is<ServiceCreatedEvent>(m => m.ReaderName == serviceName), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
            }
        }

        private void VerifyServiceDeletedEventPublished(params string[] serviceNames)
        {
            _bigBrotherMock.Verify(b => b.Publish(It.IsAny<ServiceDeletedEvent>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Exactly(serviceNames.Length));

            foreach (var serviceName in serviceNames)
            {
                _bigBrotherMock.Verify(b => b.Publish(It.Is<ServiceDeletedEvent>(m => m.ReaderName == serviceName), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
            }
        }
    }
}