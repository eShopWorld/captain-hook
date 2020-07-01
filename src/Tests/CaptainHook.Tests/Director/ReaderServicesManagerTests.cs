using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common.Configuration;
using CaptainHook.DirectorService.Events;
using CaptainHook.DirectorService.Infrastructure;
using CaptainHook.DirectorService.Infrastructure.Interfaces;
using CaptainHook.Tests.Builders;
using Eshopworld.Core;
using Eshopworld.Tests.Core;
using FluentAssertions.Execution;
using Moq;
using Xunit;

namespace CaptainHook.Tests.Director
{
    public class ReaderServicesManagerTests
    {
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

        [Fact, IsLayer0]
        public async Task CreateReadersAsync_ForFreshEnvironment_ShouldCreateAllReadersAndPublishTelemetryEvents()
        {
            var readerServiceManager = CreateReaderServiceManager();

            var subscribersToCreate = new[]
            {
                new SubscriberConfigurationBuilder().WithType("testevent").WithCallback().Create(),
                new SubscriberConfigurationBuilder().WithType("testevent").WithSubscriberName("subscriber1").Create(),
                new SubscriberConfigurationBuilder().WithType("testevent.completed").Create(),
            };

            var deployedServicesNames = Enumerable.Empty<string>();

            await readerServiceManager.CreateReadersAsync(subscribersToCreate, deployedServicesNames, _webhooks, CancellationToken.None);

            using (new AssertionScope())
            {
                VerifyFabricClientCreateCalls("fabric:/CaptainHook/EventReader.testevent-captain-hook",
                    "fabric:/CaptainHook/EventReader.testevent-subscriber1",
                    "fabric:/CaptainHook/EventReader.testevent.completed-captain-hook");

                VerifyFabricClientDeleteCalls();

                VerifyServiceCreatedEventPublished("fabric:/CaptainHook/EventReader.testevent-captain-hook",
                    "fabric:/CaptainHook/EventReader.testevent-subscriber1",
                    "fabric:/CaptainHook/EventReader.testevent.completed-captain-hook");

                VerifyServiceDeletedEventPublished();
            }
        }

        [Fact, IsLayer0]
        public async Task CreateReadersAsync_ForExistingEnvironmentWithTimeBasedSuffixes_ShouldRegenerateAllReadersAndPublishTelemetryEvents()
        {
            var readerServiceManager = CreateReaderServiceManager();

            var subscribersToCreate = new[]
            {
                new SubscriberConfigurationBuilder().WithType("testevent").WithCallback().Create(),
                new SubscriberConfigurationBuilder().WithType("testevent").WithSubscriberName("subscriber1").Create(),
                new SubscriberConfigurationBuilder().WithType("testevent.completed").Create(),
            };

            var deployedServicesNames = new[]
            {
                "fabric:/CaptainHook/EventReader.oldtestevent.completed-captain-hook-10000000000000",
                "fabric:/CaptainHook/EventReader.oldtestevent.completed-oldsubscriber-10000000000000",
                "fabric:/CaptainHook/EventReader.testevent-captain-hook-10000000000000",
                "fabric:/CaptainHook/EventReader.testevent-subscriber1-10000000000001",
                "fabric:/CaptainHook/EventReader.testevent.completed-captain-hook-10000000000000",
            };

            await readerServiceManager.CreateReadersAsync(subscribersToCreate, deployedServicesNames, _webhooks, CancellationToken.None);

            using (new AssertionScope())
            {
                VerifyFabricClientCreateCalls("fabric:/CaptainHook/EventReader.testevent-captain-hook",
                    "fabric:/CaptainHook/EventReader.testevent-subscriber1",
                    "fabric:/CaptainHook/EventReader.testevent.completed-captain-hook");

                VerifyFabricClientDeleteCalls("fabric:/CaptainHook/EventReader.oldtestevent.completed-captain-hook-10000000000000",
                    "fabric:/CaptainHook/EventReader.oldtestevent.completed-oldsubscriber-10000000000000",
                    "fabric:/CaptainHook/EventReader.testevent-captain-hook-10000000000000",
                    "fabric:/CaptainHook/EventReader.testevent-subscriber1-10000000000001",
                    "fabric:/CaptainHook/EventReader.testevent.completed-captain-hook-10000000000000");

                VerifyServiceCreatedEventPublished("fabric:/CaptainHook/EventReader.testevent-captain-hook",
                    "fabric:/CaptainHook/EventReader.testevent-subscriber1",
                    "fabric:/CaptainHook/EventReader.testevent.completed-captain-hook");

                VerifyServiceDeletedEventPublished("fabric:/CaptainHook/EventReader.oldtestevent.completed-captain-hook-10000000000000",
                    "fabric:/CaptainHook/EventReader.oldtestevent.completed-oldsubscriber-10000000000000",
                    "fabric:/CaptainHook/EventReader.testevent-captain-hook-10000000000000",
                    "fabric:/CaptainHook/EventReader.testevent-subscriber1-10000000000001",
                    "fabric:/CaptainHook/EventReader.testevent.completed-captain-hook-10000000000000");
            }
        }

        [Fact, IsLayer0]
        public async Task CreateReadersAsync_ForExistingEnvironmentWithOnlyObsoleteReaders_ShouldRegenerateAllReadersAndPublishTelemetryEvents()
        {
            var readerServiceManager = CreateReaderServiceManager();

            var subscribersToCreate = new[]
            {
                new SubscriberConfigurationBuilder().WithType("testevent").WithCallback().Create(),
                new SubscriberConfigurationBuilder().WithType("testevent").WithSubscriberName("subscriber1").Create(),
                new SubscriberConfigurationBuilder().WithType("testevent.completed").Create(),
            };

            var deployedServicesNames = new[]
            {
                "fabric:/CaptainHook/EventReader.oldtestevent.completed-captain-hook-abc",
                "fabric:/CaptainHook/EventReader.oldtestevent.completed-oldsubscriber-abc",
                "fabric:/CaptainHook/EventReader.testevent-captain-hook-abc",
                "fabric:/CaptainHook/EventReader.testevent-subscriber1-abc",
                "fabric:/CaptainHook/EventReader.testevent.completed-captain-hook-abc",
            };

            await readerServiceManager.CreateReadersAsync(subscribersToCreate, deployedServicesNames, _webhooks, CancellationToken.None);

            using (new AssertionScope())
            {
                VerifyFabricClientCreateCalls("fabric:/CaptainHook/EventReader.testevent-captain-hook",
                    "fabric:/CaptainHook/EventReader.testevent-subscriber1",
                    "fabric:/CaptainHook/EventReader.testevent.completed-captain-hook");

                VerifyFabricClientDeleteCalls(
                    "fabric:/CaptainHook/EventReader.oldtestevent.completed-captain-hook-abc",
                    "fabric:/CaptainHook/EventReader.oldtestevent.completed-oldsubscriber-abc",
                    "fabric:/CaptainHook/EventReader.testevent-captain-hook-abc",
                    "fabric:/CaptainHook/EventReader.testevent-subscriber1-abc",
                    "fabric:/CaptainHook/EventReader.testevent.completed-captain-hook-abc");

                VerifyServiceCreatedEventPublished("fabric:/CaptainHook/EventReader.testevent-captain-hook",
                    "fabric:/CaptainHook/EventReader.testevent-subscriber1",
                    "fabric:/CaptainHook/EventReader.testevent.completed-captain-hook");

                VerifyServiceDeletedEventPublished(
                    "fabric:/CaptainHook/EventReader.oldtestevent.completed-captain-hook-abc",
                    "fabric:/CaptainHook/EventReader.oldtestevent.completed-oldsubscriber-abc",
                    "fabric:/CaptainHook/EventReader.testevent-captain-hook-abc",
                    "fabric:/CaptainHook/EventReader.testevent-subscriber1-abc",
                    "fabric:/CaptainHook/EventReader.testevent.completed-captain-hook-abc");
            }
        }

        [Fact, IsLayer0]
        public async Task CreateReadersAsync_ForExistingEnvironmentWithOneNewAndTwoChangedConfigurations_ShouldRegenerateOnlyChangedReadersAndPublishTelemetryEvent()
        {
            var readerServiceManager = CreateReaderServiceManager();

            var deployedServicesNames = new[]
            {
                "fabric:/CaptainHook/EventReader.oldtestevent-captain-hook-abc",
                "fabric:/CaptainHook/EventReader.oldtestevent-oldsubscriber-abc",
                "fabric:/CaptainHook/EventReader.testevent-captain-hook-1XyvgNnSUnqLqKs79dCfku",
                "fabric:/CaptainHook/EventReader.testevent-subscriber1-abc",
                "fabric:/CaptainHook/EventReader.testevent.completed-captain-hook-1fTs0vQq8JT0fMU6tZPL2z",
            };

            var webhooks = new[]
            {
                new WebhookConfig { Name = "testevent"},
                new WebhookConfig { Name = "testevent.completed"},
                new WebhookConfig { Name = "oldtestevent"}
            };

            var newSubscribersMap = new[]
            {
                // Not touch:
                new SubscriberConfigurationBuilder().WithType("testevent").WithCallback().Create(),
                new SubscriberConfigurationBuilder().WithType("testevent.completed").Create(),

                // To create and delete:
                new SubscriberConfigurationBuilder().WithType("testevent").WithCallback().WithSubscriberName("subscriber1").Create(),
                new SubscriberConfigurationBuilder().WithType("oldtestevent").WithCallback().WithOidcAuthentication().Create(),

                // To create:
                new SubscriberConfigurationBuilder().WithType("newtestevent").WithCallback().Create(),
            };

            await readerServiceManager.CreateReadersAsync(newSubscribersMap, deployedServicesNames, webhooks, CancellationToken.None);

            using (new AssertionScope())
            {
                VerifyFabricClientCreateCalls(
                    "fabric:/CaptainHook/EventReader.newtestevent-captain-hook",
                    "fabric:/CaptainHook/EventReader.testevent-subscriber1",
                    "fabric:/CaptainHook/EventReader.oldtestevent-captain-hook");

                VerifyFabricClientDeleteCalls(
                    "fabric:/CaptainHook/EventReader.oldtestevent-oldsubscriber-abc",
                    "fabric:/CaptainHook/EventReader.testevent-subscriber1-abc",
                    "fabric:/CaptainHook/EventReader.oldtestevent-captain-hook-abc");

                VerifyServiceCreatedEventPublished(
                    "fabric:/CaptainHook/EventReader.newtestevent-captain-hook",
                    "fabric:/CaptainHook/EventReader.testevent-subscriber1",
                    "fabric:/CaptainHook/EventReader.oldtestevent-captain-hook");

                VerifyServiceDeletedEventPublished(
                    "fabric:/CaptainHook/EventReader.oldtestevent-oldsubscriber-abc",
                    "fabric:/CaptainHook/EventReader.testevent-subscriber1-abc",
                    "fabric:/CaptainHook/EventReader.oldtestevent-captain-hook-abc");
            }
        }

        [Fact, IsLayer0]
        public async Task RefreshReadersAsync_ForExistingEnvironment_ShouldCallFabricClientWrapperAndPublishTelemetryEvent()
        {
            var readerServiceManager = CreateReaderServiceManager();

            var deployedServicesNames = new[]
            {
                "fabric:/CaptainHook/EventReader.oldtestevent-captain-hook-a",
                "fabric:/CaptainHook/EventReader.oldtestevent-oldsubscriber-a",
                "fabric:/CaptainHook/EventReader.testevent-captain-hook-1XyvgNnSUnqLqKs79dCfku",
                "fabric:/CaptainHook/EventReader.testevent-subscriber1-a",
                "fabric:/CaptainHook/EventReader.testevent.completed-captain-hook-1fTs0vQq8JT0fMU6tZPL2z",
            };

            var webhooks = new[]
            {
                new WebhookConfig { Name = "testevent"},
                new WebhookConfig { Name = "testevent.completed"},
                new WebhookConfig { Name = "oldtestevent"}
            };

            var currentSubscribersMap = new Dictionary<string, SubscriberConfiguration>
            {
                // To delete:
                ["oldtestevent;oldsubscriber"] = new SubscriberConfigurationBuilder().WithType("oldtestevent").WithSubscriberName("oldsubscriber").Create(),

                // Not touch:
                ["testevent;captain-hook"] = new SubscriberConfigurationBuilder().WithType("testevent").WithCallback().Create(),
                ["testevent.completed;captain-hook"] = new SubscriberConfigurationBuilder().WithType("testevent.completed").Create(),

                // To create and delete:
                ["testevent;subscriber1"] = new SubscriberConfigurationBuilder().WithType("testevent").WithSubscriberName("subscriber1").Create(),
                ["oldtestevent;captain-hook"] = new SubscriberConfigurationBuilder().WithType("oldtestevent").WithCallback().Create(),
            };

            var newSubscribersMap = new Dictionary<string, SubscriberConfiguration>
            {
                // Not touch:
                ["testevent;captain-hook"] = new SubscriberConfigurationBuilder().WithType("testevent").WithCallback().Create(),
                ["testevent.completed;captain-hook"] = new SubscriberConfigurationBuilder().WithType("testevent.completed").Create(),

                // To create and delete:
                ["testevent;subscriber1"] = new SubscriberConfigurationBuilder().WithType("testevent").WithCallback().WithSubscriberName("subscriber1").Create(),
                ["oldtestevent;captain-hook"] = new SubscriberConfigurationBuilder().WithType("oldtestevent").WithCallback().WithOidcAuthentication().Create(),

                // To create:
                ["newtestevent;captain-hook"] = new SubscriberConfigurationBuilder().WithType("newtestevent").WithCallback().Create(),
            };

            await readerServiceManager.RefreshReadersAsync(newSubscribersMap, webhooks, currentSubscribersMap, deployedServicesNames, CancellationToken.None);

            using (new AssertionScope())
            {
                VerifyFabricClientCreateCalls(
                    "fabric:/CaptainHook/EventReader.newtestevent-captain-hook",
                    "fabric:/CaptainHook/EventReader.testevent-subscriber1",
                    "fabric:/CaptainHook/EventReader.oldtestevent-captain-hook");

                VerifyFabricClientDeleteCalls(
                    "fabric:/CaptainHook/EventReader.oldtestevent-oldsubscriber-a",
                    "fabric:/CaptainHook/EventReader.testevent-subscriber1-a",
                    "fabric:/CaptainHook/EventReader.oldtestevent-captain-hook-a");

                VerifyServiceCreatedEventPublished(
                    "fabric:/CaptainHook/EventReader.newtestevent-captain-hook",
                    "fabric:/CaptainHook/EventReader.testevent-subscriber1",
                    "fabric:/CaptainHook/EventReader.oldtestevent-captain-hook");

                VerifyServiceDeletedEventPublished(
                    "fabric:/CaptainHook/EventReader.oldtestevent-oldsubscriber-a",
                    "fabric:/CaptainHook/EventReader.testevent-subscriber1-a",
                    "fabric:/CaptainHook/EventReader.oldtestevent-captain-hook-a");

                _bigBrotherMock.Verify(b => b.Publish(
                    It.Is<RefreshSubscribersEvent>(m => m.AddedCount == 1 && m.RemovedCount == 1 && m.ChangedCount == 2),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
            }
        }

        private void VerifyFabricClientCreateCalls(params string[] serviceNames)
        {
            _fabricClientMock.Verify(c => c.CreateServiceAsync(It.IsAny<ServiceCreationDescription>(), It.IsAny<CancellationToken>()), Times.Exactly(serviceNames.Length));

            foreach (var serviceName in serviceNames)
            {
                _fabricClientMock.Verify(c => c.CreateServiceAsync(It.Is<ServiceCreationDescription>(
                    m => Regex.IsMatch(m.ServiceName, $"{serviceName}-([a-zA-Z0-9]{{22}})")), It.IsAny<CancellationToken>()), Times.Once);
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

        private void VerifyServiceCreatedEventPublished(params string[] serviceNames)
        {
            _bigBrotherMock.Verify(b => b.Publish(It.IsAny<ServiceCreatedEvent>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Exactly(serviceNames.Length));

            foreach (var serviceName in serviceNames)
            {
                _bigBrotherMock.Verify(b => b.Publish(It.Is<ServiceCreatedEvent>(
                    m => Regex.IsMatch(m.ReaderName, $"{serviceName}-([a-zA-Z0-9]{{22}})")), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
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