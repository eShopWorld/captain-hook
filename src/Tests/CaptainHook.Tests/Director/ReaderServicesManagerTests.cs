using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common.Configuration;
using CaptainHook.DirectorService;
using CaptainHook.DirectorService.Events;
using CaptainHook.DirectorService.Utils;
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

        private Mock<IBigBrother> _bigBrotherMock = new Mock<IBigBrother>();
        private Mock<IFabricClientWrapper> _fabricClientMock = new Mock<IFabricClientWrapper>();


        [Fact, IsLayer0]
        public async Task CreateReadersAsync_ForFreshEnvironment_ShouldCallFabricClientWrapperToCreateNewInstances()
        {
            var readerServiceManager = new ReaderServicesManager(_fabricClientMock.Object, _bigBrotherMock.Object);

            var deployedServicesNames = Enumerable.Empty<string>();

            await readerServiceManager.CreateReadersAsync(_subscribersToCreate, deployedServicesNames, _webhooks, CancellationToken.None);

            VerifyFabricClientCreateCalls("fabric:/CaptainHook/EventReader.testevent-captain-hook-a",
                "fabric:/CaptainHook/EventReader.testevent-subscriber1-a",
                "fabric:/CaptainHook/EventReader.testevent.completed-captain-hook-a");

            VerifyFabricClientDeleteCalls();
        }

        [Fact, IsLayer0]
        public async Task CreateReadersAsync_ForExistingEnvironment_ShouldCallFabricClientWrapperToCreateNewInstances()
        {
            var readerServiceManager = new ReaderServicesManager(_fabricClientMock.Object, _bigBrotherMock.Object);

            var deployedServicesNames = new[]
            {
                "fabric:/CaptainHook/EventReader.oldtestevent.completed-captain-hook-a",
                "fabric:/CaptainHook/EventReader.oldtestevent.completed-oldsubscriber-a",
                "fabric:/CaptainHook/EventReader.testevent-captain-hook-a",
                "fabric:/CaptainHook/EventReader.testevent-subscriber1-b",
                "fabric:/CaptainHook/EventReader.testevent.completed-captain-hook-a",
            };

            await readerServiceManager.CreateReadersAsync(_subscribersToCreate, deployedServicesNames, _webhooks, CancellationToken.None);

            VerifyFabricClientCreateCalls("fabric:/CaptainHook/EventReader.testevent-captain-hook-b",
                "fabric:/CaptainHook/EventReader.testevent-subscriber1-a",
                "fabric:/CaptainHook/EventReader.testevent.completed-captain-hook-b");

            VerifyFabricClientDeleteCalls("fabric:/CaptainHook/EventReader.testevent-captain-hook-a",
                "fabric:/CaptainHook/EventReader.testevent-subscriber1-b",
                "fabric:/CaptainHook/EventReader.testevent.completed-captain-hook-a");
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
            var readerServiceManager = new ReaderServicesManager(_fabricClientMock.Object, _bigBrotherMock.Object);

            var deployedServicesNames = Enumerable.Empty<string>();

            await readerServiceManager.CreateReadersAsync(_subscribersToCreate, deployedServicesNames, _webhooks, CancellationToken.None);

            VerifyServiceCreatedEventPublished("fabric:/CaptainHook/EventReader.testevent-captain-hook-a",
                "fabric:/CaptainHook/EventReader.testevent-subscriber1-a",
                "fabric:/CaptainHook/EventReader.testevent.completed-captain-hook-a");

            VerifyServiceDeletedEventPublished();
        }

        [Fact, IsLayer0]
        public async Task CreateReadersAsync_ForExistingEnvironment_ShouldPublishTelemetryEvents()
        {
            var readerServiceManager = new ReaderServicesManager(_fabricClientMock.Object, _bigBrotherMock.Object);

            var deployedServicesNames = new[]
            {
                "fabric:/CaptainHook/EventReader.oldtestevent.completed-captain-hook-a",
                "fabric:/CaptainHook/EventReader.oldtestevent.completed-oldsubscriber-a",
                "fabric:/CaptainHook/EventReader.testevent-captain-hook-a",
                "fabric:/CaptainHook/EventReader.testevent-subscriber1-b",
                "fabric:/CaptainHook/EventReader.testevent.completed-captain-hook-a",
            };

            await readerServiceManager.CreateReadersAsync(_subscribersToCreate, deployedServicesNames, _webhooks, CancellationToken.None);

            VerifyServiceCreatedEventPublished("fabric:/CaptainHook/EventReader.testevent-captain-hook-b",
                "fabric:/CaptainHook/EventReader.testevent-subscriber1-a",
                "fabric:/CaptainHook/EventReader.testevent.completed-captain-hook-b");

            VerifyServiceDeletedEventPublished("fabric:/CaptainHook/EventReader.testevent-captain-hook-a",
                "fabric:/CaptainHook/EventReader.testevent-subscriber1-b",
                "fabric:/CaptainHook/EventReader.testevent.completed-captain-hook-a");
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