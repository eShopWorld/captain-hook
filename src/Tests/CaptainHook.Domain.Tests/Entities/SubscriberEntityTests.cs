using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Results;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Xunit;

namespace CaptainHook.Domain.Tests.Entities
{
    public class SubscriberEntityTests
    {
        [Fact, IsUnit]
        public void UpsertWebhookEndpoint_SingleEndpointAdded_ResultIsNull()
        {
            // Arrange
            var subscriberEntity = new SubscriberEntity("testName");
            var endpointEntity = new EndpointEntity("uri", null, "PUT", "*");

            // Act
            var result = subscriberEntity.SetWebhookEndpoint(endpointEntity);

            // Assert
            result.Should().BeOfType<OperationResult<SubscriberEntity>>()
                .Which.Data.Should().BeSameAs(subscriberEntity, "because successful upsert returns subscriber itself");
        }

        [Fact, IsUnit]
        public void UpsertWebhookEndpoint_SingleEndpointAdded_EndpointOnList()
        {
            // Arrange
            var subscriberEntity = new SubscriberEntity("testName");
            var endpointEntity = new EndpointEntity("uri", null, "PUT", "*");

            // Act
            var result = subscriberEntity.SetWebhookEndpoint(endpointEntity);

            // Assert
            result.Data.Webhooks.Endpoints.Should().BeEquivalentTo(
                new[] { endpointEntity },
                options => options.IgnoringCyclicReferences());
        }

        [Fact, IsUnit]
        public void UpsertWebhookEndpoint_MultipleEndpointsAdded_EndpointsOnList()
        {
            // Arrange
            var subscriberEntity = new SubscriberEntity("testName");
            var endpointEntity1 = new EndpointEntity("uri", null, "PUT", "*");
            subscriberEntity.AddWebhooks(new WebhooksEntity("$.Test", new[] { endpointEntity1 }));
            var endpointEntity2 = new EndpointEntity("uri2", null, "DELETE", "abc");

            // Act
            var result = subscriberEntity.SetWebhookEndpoint(endpointEntity2);

            // Assert
            result.Data.Webhooks.Endpoints.Should().BeEquivalentTo(
                new[] { endpointEntity1, endpointEntity2 },
                options => options.IgnoringCyclicReferences().WithStrictOrdering());
        }

        [Fact, IsUnit]
        public void UpsertWebhookEndpoint_EndpointUpdated_EndpointOnList()
        {
            // Arrange
            var subscriberEntity = new SubscriberEntity("testName");
            var endpointEntity1 = new EndpointEntity("uri", null, "PUT", "*");
            var endpointEntity2 = new EndpointEntity("uri2", null, "DELETE", "*");
            subscriberEntity.SetWebhookEndpoint(endpointEntity1);

            // Act
            var result = subscriberEntity.SetWebhookEndpoint(endpointEntity2);

            // Assert
            result.Data.Webhooks.Endpoints.Should().BeEquivalentTo(
                new[] { endpointEntity2 },
                options => options.IgnoringCyclicReferences());
        }
    }
}