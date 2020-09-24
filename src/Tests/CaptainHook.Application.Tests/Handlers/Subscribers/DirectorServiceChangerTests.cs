using System.Threading.Tasks;
using CaptainHook.Application.Handlers.Subscribers;
using CaptainHook.Application.Infrastructure.DirectorService;
using CaptainHook.Common.Configuration;
using CaptainHook.Domain.Entities;
using CaptainHook.TestsInfrastructure.Builders;
using Eshopworld.Tests.Core;
using Moq;
using Xunit;

namespace CaptainHook.Application.Tests.Handlers.Subscribers
{
    public class DirectorServiceChangerTests
    {
        private readonly Mock<IDirectorServiceProxy> _directorServiceMock = new Mock<IDirectorServiceProxy>(MockBehavior.Strict);

        private DirectorServiceChanger Changer => new DirectorServiceChanger(_directorServiceMock.Object);

        public DirectorServiceChangerTests()
        {
            _directorServiceMock.Setup(x => x.CreateReaderAsync(It.IsAny<SubscriberEntity>())).ReturnsAsync(new SubscriberConfiguration());
            _directorServiceMock.Setup(x => x.UpdateReaderAsync(It.IsAny<SubscriberEntity>())).ReturnsAsync(new SubscriberConfiguration());
            _directorServiceMock.Setup(x => x.DeleteReaderAsync(It.IsAny<SubscriberEntity>())).ReturnsAsync(new SubscriberConfiguration());
            _directorServiceMock.Setup(x => x.CreateDlqReaderAsync(It.IsAny<SubscriberEntity>())).ReturnsAsync(new SubscriberConfiguration());
            _directorServiceMock.Setup(x => x.UpdateDlqReaderAsync(It.IsAny<SubscriberEntity>())).ReturnsAsync(new SubscriberConfiguration());
            _directorServiceMock.Setup(x => x.DeleteDlqReaderAsync(It.IsAny<SubscriberEntity>())).ReturnsAsync(new SubscriberConfiguration());
        }

        [Fact, IsUnit]
        public async Task When_NewSubscriberWithoutDlq_ThenOnlyWebhookReaderCreated()
        {
            var subscriber = new SubscriberBuilder()
                .WithWebhook("https://blah-blah.eshopworld.com/webhook/", "PUT", "*")
                .Create();

            await Changer.ApplyAsync(null, subscriber);

            _directorServiceMock.Verify(x => x.CreateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Once);
            _directorServiceMock.Verify(x => x.UpdateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Never);
            _directorServiceMock.Verify(x => x.DeleteReaderAsync(It.IsAny<SubscriberEntity>()), Times.Never);
            _directorServiceMock.Verify(x => x.CreateDlqReaderAsync(It.IsAny<SubscriberEntity>()), Times.Never);
            _directorServiceMock.Verify(x => x.UpdateDlqReaderAsync(It.IsAny<SubscriberEntity>()), Times.Never);
            _directorServiceMock.Verify(x => x.DeleteDlqReaderAsync(It.IsAny<SubscriberEntity>()), Times.Never);
        }

        [Fact, IsUnit]
        public async Task When_NewSubscriberWithDlq_ThenWebhookAndDlqReadersCreated()
        {
            var subscriber = new SubscriberBuilder()
               .WithWebhook("https://blah-blah.eshopworld.com/webhook/", "PUT", "*")
               .WithDlqhook("https://blah-blah.eshopworld.com/dlq/", "PUT", "*")
               .Create();

            await Changer.ApplyAsync(null, subscriber);

            _directorServiceMock.Verify(x => x.CreateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Once);
            _directorServiceMock.Verify(x => x.UpdateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Never);
            _directorServiceMock.Verify(x => x.DeleteReaderAsync(It.IsAny<SubscriberEntity>()), Times.Never);
            _directorServiceMock.Verify(x => x.CreateDlqReaderAsync(It.IsAny<SubscriberEntity>()), Times.Once);
            _directorServiceMock.Verify(x => x.UpdateDlqReaderAsync(It.IsAny<SubscriberEntity>()), Times.Never);
            _directorServiceMock.Verify(x => x.DeleteDlqReaderAsync(It.IsAny<SubscriberEntity>()), Times.Never);
        }


        [Fact, IsUnit]
        public async Task When_AddingDlqToExistingSubscriber_ThenWebhookUpdatedAndDlqReaderCreated()
        {
            var existingSubscriber = new SubscriberBuilder()
                .WithWebhook("https://blah-blah.eshopworld.com/webhook/", "PUT", "*")
                .Create();

            var subscriber = new SubscriberBuilder()
               .WithWebhook("https://blah-blah.eshopworld.com/webhook/", "POST", "*")
               .WithDlqhook("https://blah-blah.eshopworld.com/dlq/", "PUT", "*")
               .Create();

            await Changer.ApplyAsync(existingSubscriber, subscriber);

            _directorServiceMock.Verify(x => x.CreateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Never);
            _directorServiceMock.Verify(x => x.UpdateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Once);
            _directorServiceMock.Verify(x => x.DeleteReaderAsync(It.IsAny<SubscriberEntity>()), Times.Never);
            _directorServiceMock.Verify(x => x.CreateDlqReaderAsync(It.IsAny<SubscriberEntity>()), Times.Once);
            _directorServiceMock.Verify(x => x.UpdateDlqReaderAsync(It.IsAny<SubscriberEntity>()), Times.Never);
            _directorServiceMock.Verify(x => x.DeleteDlqReaderAsync(It.IsAny<SubscriberEntity>()), Times.Never);
        }

        [Fact, IsUnit]
        public async Task When_UpdatingDlqInExistingSubscriber_ThenWebhookUpdatedAndDlqReaderUpdated()
        {
            var existingSubscriber = new SubscriberBuilder()
               .WithWebhook("https://blah-blah.eshopworld.com/webhook/", "PUT", "*")
               .WithDlqhook("https://blah-blah.eshopworld.com/dlq/", "PUT", "*")
               .Create();

            var subscriber = new SubscriberBuilder()
               .WithWebhook("https://blah-blah.eshopworld.com/webhook/", "POST", "*")
               .WithDlqhook("https://blah-blah.eshopworld.com/dlq/", "POST", "*")
               .Create();

            await Changer.ApplyAsync(existingSubscriber, subscriber);

            _directorServiceMock.Verify(x => x.CreateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Never);
            _directorServiceMock.Verify(x => x.UpdateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Once);
            _directorServiceMock.Verify(x => x.DeleteReaderAsync(It.IsAny<SubscriberEntity>()), Times.Never);
            _directorServiceMock.Verify(x => x.CreateDlqReaderAsync(It.IsAny<SubscriberEntity>()), Times.Never);
            _directorServiceMock.Verify(x => x.UpdateDlqReaderAsync(It.IsAny<SubscriberEntity>()), Times.Once);
            _directorServiceMock.Verify(x => x.DeleteDlqReaderAsync(It.IsAny<SubscriberEntity>()), Times.Never);
        }

        [Fact, IsUnit]
        public async Task When_RemovingDlqFromExistingSubscriber_ThenWebhookUpdatedAndDlqReaderRemoved()
        {
            var existingSubscriber = new SubscriberBuilder()
               .WithWebhook("https://blah-blah.eshopworld.com/webhook/", "PUT", "*")
               .WithDlqhook("https://blah-blah.eshopworld.com/dlq/", "PUT", "*")
               .Create();

            var subscriber = new SubscriberBuilder()
               .WithWebhook("https://blah-blah.eshopworld.com/webhook/", "POST", "*")
               .Create();

            await Changer.ApplyAsync(existingSubscriber, subscriber);

            _directorServiceMock.Verify(x => x.CreateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Never);
            _directorServiceMock.Verify(x => x.UpdateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Once);
            _directorServiceMock.Verify(x => x.DeleteReaderAsync(It.IsAny<SubscriberEntity>()), Times.Never);
            _directorServiceMock.Verify(x => x.CreateDlqReaderAsync(It.IsAny<SubscriberEntity>()), Times.Never);
            _directorServiceMock.Verify(x => x.UpdateDlqReaderAsync(It.IsAny<SubscriberEntity>()), Times.Never);
            _directorServiceMock.Verify(x => x.DeleteDlqReaderAsync(It.IsAny<SubscriberEntity>()), Times.Once);
        }
    }
}