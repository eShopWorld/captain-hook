using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common.Configuration;
using CaptainHook.DirectorService;
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
        [Fact, IsLayer0]
        public async Task CreateReadersAsync_ForFreshEnvironment_ShouldCallFabricClientMockToCreateNewInstances()
        {
            var bigBrotherMock = new Mock<IBigBrother>();
            var fabricClientMock = new Mock<IFabricClientWrapper>();

            var readerServiceManager = new ReaderServicesManager(fabricClientMock.Object, bigBrotherMock.Object);

            var subscribersToCreate = new[]
            {
                new SubscriberConfigurationBuilder().WithType("testevent").WithCallback().Create(),
                new SubscriberConfigurationBuilder().WithType("testevent").WithSubscriberName("subscriber1").Create(),
                new SubscriberConfigurationBuilder().WithType("testevent.completed").Create(),
            };

            var webhooks = new[]
            {
                new WebhookConfig { Name = "testevent" },
                new WebhookConfig { Name = "testevent.completed" }
            };

            var deployedServicesNames = Enumerable.Empty<string>();

            await readerServiceManager.CreateReadersAsync(subscribersToCreate, deployedServicesNames, webhooks, CancellationToken.None);

            fabricClientMock.Verify(c => c.CreateServiceAsync(It.IsAny<ServiceCreationDescription>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
            fabricClientMock.Verify(c => c.CreateServiceAsync(It.Is<ServiceCreationDescription>(m => m.ServiceName == "fabric:/CaptainHook/EventReader.testevent-captain-hook-a"), It.IsAny<CancellationToken>()), Times.Once);
            fabricClientMock.Verify(c => c.CreateServiceAsync(It.Is<ServiceCreationDescription>(m => m.ServiceName == "fabric:/CaptainHook/EventReader.testevent-subscriber1-a"), It.IsAny<CancellationToken>()), Times.Once);
            fabricClientMock.Verify(c => c.CreateServiceAsync(It.Is<ServiceCreationDescription>(m => m.ServiceName == "fabric:/CaptainHook/EventReader.testevent.completed-captain-hook-a"), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}