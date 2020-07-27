using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Application.Handlers.Subscribers;
using CaptainHook.Application.Requests.Subscribers;
using CaptainHook.Common.Remoting;
using CaptainHook.Domain.Repositories;
using CaptainHook.TestsInfrastructure.Builders;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Moq;
using Xunit;

namespace CaptainHook.Application.Tests.Handlers
{
    public class UpsertWebhookRequestHandlerTests
    {
        [Fact, IsUnit]
        public async Task When_Subscriber_does_not_exist_then_new_one_will_be_created()
        {
            var repositoryMock = new Mock<ISubscriberRepository>();
            var directorServiceMock = new Mock<IDirectorServiceRemoting>();
            var handler = new UpsertWebhookRequestHandler(repositoryMock.Object, directorServiceMock.Object);
            var request = new UpsertWebhookRequest("event", "subscriber", new EndpointDtoBuilder().Create());

            var result = await handler.Handle(request, CancellationToken.None);

            result.IsError.Should().BeFalse();
        }

        [Fact, IsUnit]
        public async Task When_Subscriber_does_exist_then_will_be_updated_with_new_WebHook()
        {
        }

        [Fact, IsUnit]
        public async Task When_Repository_fails_retrieving_Subscriber_data_then_operation_fails()
        {
        }

        [Fact, IsUnit]
        public async Task When_DirectorService_is_busy_reloading_then_operation_fails()
        {
        }

        [Fact, IsUnit]
        public async Task When_DirectorService_fails_during_Reader_creation_then_operation_fails()
        {
        }

        [Fact, IsUnit]
        public async Task When_Repository_fails_saving_Subscriber_after_succesfull_Reader_creation_then_operation_fails()
        {
        }
    }
}
