using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly Mock<IBigBrother> _bigBrotherMock = new Mock<IBigBrother>();
        private readonly Mock<IFabricClientWrapper> _fabricClientMock = new Mock<IFabricClientWrapper>();

        private ReaderServicesManager CreateReaderServiceManager()
        {
            return new ReaderServicesManager(_fabricClientMock.Object, _bigBrotherMock.Object);
        }

        [Fact, IsLayer0]
        public async Task RefreshReadersAsync_ForFreshEnvironment_ShouldCreateAllReadersAndPublishTelemetryEvents()
        {
            var readerServiceManager = CreateReaderServiceManager();

            var subscribersToCreate = new[]
            {
                new SubscriberConfigurationBuilder().WithType("testevent").WithCallback().Create(),
                new SubscriberConfigurationBuilder().WithType("testevent").WithSubscriberName("subscriber1").Create(),
                new SubscriberConfigurationBuilder().WithType("testevent.completed").Create(),
            };

            var deployedServicesNames = Enumerable.Empty<string>();

            await readerServiceManager.RefreshReadersAsync(subscribersToCreate, deployedServicesNames, CancellationToken.None);

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

                _bigBrotherMock.Verify(b => b.Publish(
                    It.Is<RefreshSubscribersEvent>(m => m.AddedCount == 3 && m.RemovedCount == 0 && m.ChangedCount == 0),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
            }
        }

        [Fact, IsLayer0]
        public async Task RefreshReadersAsync_ForExistingEnvironmentWithTimeBasedSuffixes_ShouldRegenerateAllReadersAndPublishTelemetryEvents()
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
                "fabric:/CaptainHook/CaptainHook.ApiPkg",
                "fabric:/CaptainHook/CaptainHook.DirectorService",
                "fabric:/CaptainHook/EventReader.oldtestevent.completed-captain-hook-10000000000000",
                "fabric:/CaptainHook/EventReader.oldtestevent.completed-oldsubscriber-10000000000000",
                "fabric:/CaptainHook/EventReader.testevent-captain-hook-10000000000000",
                "fabric:/CaptainHook/EventReader.testevent-subscriber1-10000000000001",
                "fabric:/CaptainHook/EventReader.testevent.completed-captain-hook-10000000000000",
            };

            await readerServiceManager.RefreshReadersAsync(subscribersToCreate, deployedServicesNames, CancellationToken.None);

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

                _bigBrotherMock.Verify(b => b.Publish(
                    It.Is<RefreshSubscribersEvent>(m => m.AddedCount == 0 && m.RemovedCount == 2 && m.ChangedCount == 3),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
            }
        }

        [Fact, IsLayer0]
        public async Task RefreshReadersAsync_ForExistingEnvironmentWithOnlyObsoleteReaders_ShouldRegenerateAllReadersAndPublishTelemetryEvents()
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
                "fabric:/CaptainHook/CaptainHook.ApiPkg",
                "fabric:/CaptainHook/CaptainHook.DirectorService",
                "fabric:/CaptainHook/EventReader.oldtestevent.completed-captain-hook-abc",
                "fabric:/CaptainHook/EventReader.oldtestevent.completed-oldsubscriber-abc",
                "fabric:/CaptainHook/EventReader.testevent-captain-hook-abc",
                "fabric:/CaptainHook/EventReader.testevent-subscriber1-abc",
                "fabric:/CaptainHook/EventReader.testevent.completed-captain-hook-abc",
            };

            await readerServiceManager.RefreshReadersAsync(subscribersToCreate, deployedServicesNames, CancellationToken.None);

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

                _bigBrotherMock.Verify(b => b.Publish(
                    It.Is<RefreshSubscribersEvent>(m => m.AddedCount == 3 && m.RemovedCount == 5 && m.ChangedCount == 0),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
            }
        }

        [Fact, IsLayer0]
        public async Task RefreshReadersAsync_ForExistingEnvironmentWithOneNewAndTwoChangedConfigurations_ShouldRefreshOnlyChangedReadersAndPublishTelemetryEvent()
        {
            var readerServiceManager = CreateReaderServiceManager();

            var existingServicesNames = new[]
            {
                "fabric:/CaptainHook/CaptainHook.ApiPkg",
                "fabric:/CaptainHook/CaptainHook.DirectorService",
                "fabric:/CaptainHook/EventReader.oldtestevent-captain-hook-a",
                "fabric:/CaptainHook/EventReader.oldtestevent-oldsubscriber-a",
                "fabric:/CaptainHook/EventReader.testevent-captain-hook-1XyvgNnSUnqLqKs79dCfku",
                "fabric:/CaptainHook/EventReader.testevent-subscriber1-a",
                "fabric:/CaptainHook/EventReader.very.old.and.obsolete.reader1",
                "fabric:/CaptainHook/EventReader.testevent.completed-captain-hook-1fTs0vQq8JT0fMU6tZPL2z",
                "fabric:/CaptainHook/EventReader.very.old.and.obsolete.reader2",
            };

            var subscribers = new[]
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

            await readerServiceManager.RefreshReadersAsync(subscribers, existingServicesNames, CancellationToken.None);

            using (new AssertionScope())
            {
                VerifyFabricClientCreateCalls(
                    "fabric:/CaptainHook/EventReader.newtestevent-captain-hook",
                    "fabric:/CaptainHook/EventReader.testevent-subscriber1",
                    "fabric:/CaptainHook/EventReader.oldtestevent-captain-hook");

                VerifyFabricClientDeleteCalls(
                    "fabric:/CaptainHook/EventReader.oldtestevent-oldsubscriber-a",
                    "fabric:/CaptainHook/EventReader.testevent-subscriber1-a",
                    "fabric:/CaptainHook/EventReader.oldtestevent-captain-hook-a",
                    "fabric:/CaptainHook/EventReader.very.old.and.obsolete.reader1",
                    "fabric:/CaptainHook/EventReader.very.old.and.obsolete.reader2");

                VerifyServiceCreatedEventPublished(
                    "fabric:/CaptainHook/EventReader.newtestevent-captain-hook",
                    "fabric:/CaptainHook/EventReader.testevent-subscriber1",
                    "fabric:/CaptainHook/EventReader.oldtestevent-captain-hook");

                VerifyServiceDeletedEventPublished(
                    "fabric:/CaptainHook/EventReader.oldtestevent-oldsubscriber-a",
                    "fabric:/CaptainHook/EventReader.testevent-subscriber1-a",
                    "fabric:/CaptainHook/EventReader.oldtestevent-captain-hook-a",
                    "fabric:/CaptainHook/EventReader.very.old.and.obsolete.reader1",
                    "fabric:/CaptainHook/EventReader.very.old.and.obsolete.reader2");

                _bigBrotherMock.Verify(b => b.Publish(
                    It.Is<RefreshSubscribersEvent>(m => m.AddedCount == 1 && m.RemovedCount == 3 && m.ChangedCount == 2),
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
            _bigBrotherMock.Verify (b => b.Publish(
                    It.IsAny<ReaderServicesDeletionEvent>(), 
                    It.IsAny<string>(), 
                    It.IsAny<string>(), 
                    It.IsAny<int>()), 
                Times.Exactly (serviceNames.Length > 0? 1: 0));

            foreach (var serviceName in serviceNames)
            {
                _bigBrotherMock.Verify (b => b.Publish(
                        It.Is<ReaderServicesDeletionEvent> (m => m.DeletedNames.Contains (serviceName) || m.Failed.Contains (serviceName)), 
                        It.IsAny<string>(), 
                        It.IsAny<string>(), 
                        It.IsAny<int>()), 
                    Times.Once);
            }
        }
    }
}