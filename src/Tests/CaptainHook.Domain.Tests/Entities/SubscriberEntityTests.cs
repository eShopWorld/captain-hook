using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Errors;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Xunit;

namespace CaptainHook.Domain.Tests.Entities
{
    public class SubscriberEntityTests
    {
        [Fact, IsUnit]
        public void SetWebhookEndpoint_SingleEndpointAdded_EndpointOnList()
        {
            // Arrange
            var subscriberEntity = new SubscriberEntity("testName", new EventEntity("eventName"));
            var endpointEntity = new EndpointEntity("uri", null, "PUT", "*");

            // Act
            var result = subscriberEntity.SetWebhookEndpoint(endpointEntity);

            // Assert
            result.Data.Webhooks.Endpoints.Should().BeEquivalentTo(
                new[] { endpointEntity },
                options => options.IgnoringCyclicReferences());
        }

        [Fact, IsUnit]
        public void SetWebhookEndpoint_MultipleEndpointsAdded_EndpointsOnList()
        {
            // Arrange
            var subscriberEntity = new SubscriberEntity("testName", new EventEntity("eventName"));
            var endpointEntity1 = new EndpointEntity("uri", null, "PUT", "*");
            subscriberEntity.SetHooks(new WebhooksEntity(WebhooksEntityType.Webhooks, "$.Test", new[] { endpointEntity1 }));
            var endpointEntity2 = new EndpointEntity("uri2", null, "DELETE", "abc");

            // Act
            var result = subscriberEntity.SetWebhookEndpoint(endpointEntity2);

            // Assert
            result.Data.Webhooks.Endpoints.Should().BeEquivalentTo(
                new[] { endpointEntity1, endpointEntity2 },
                options => options.IgnoringCyclicReferences().WithStrictOrdering());
        }

        [Fact, IsUnit]
        public void SetWebhookEndpoint_EndpointUpdated_EndpointOnList()
        {
            // Arrange
            var subscriberEntity = new SubscriberEntity("testName", new EventEntity("eventName"));
            var endpointEntity1 = new EndpointEntity("uri", null, "PUT", "*");
            subscriberEntity.SetWebhookEndpoint(endpointEntity1);

            // Act
            var endpointEntity2 = new EndpointEntity("uri2", null, "DELETE", "*");
            var result = subscriberEntity.SetWebhookEndpoint(endpointEntity2);

            // Assert
            result.Data.Webhooks.Endpoints.Should().BeEquivalentTo(
                new[] { endpointEntity2 },
                options => options.IgnoringCyclicReferences());
        }

        [Fact, IsUnit]
        public void RemoveWebhookEndpoint_WhenTwoEndpointsAreDefined_ThenOneIsRemoved()
        {
            var subscriber = new SubscriberEntity("testName", new EventEntity("eventName"));
            var endpoint = new EndpointEntity("uri", null, "POST", "*");
            subscriber.SetHooks(new WebhooksEntity(WebhooksEntityType.Webhooks, "$.Test", new[] { endpoint }, null));
            var newEndpoint = new EndpointEntity("new-uri", null, "POST", "sel");
            subscriber.SetWebhookEndpoint(newEndpoint);

            var result = subscriber.RemoveWebhookEndpoint(newEndpoint);

            result.Data.Webhooks.Endpoints.Should().BeEquivalentTo(
                new[] { endpoint },
                options => options.IgnoringCyclicReferences());
        }

        [Fact, IsUnit]
        public void RemoveWebhookEndpoint_WhenOnlyOneEndpointDefined_ThenCannotRemoveLastEndpointFromSubscriberErrorIsReturned()
        {
            var subscriber = new SubscriberEntity("testName", new EventEntity("eventName"));
            var endpoint = new EndpointEntity("uri", null, "POST", "*");
            subscriber.SetWebhookEndpoint(endpoint);

            var result = subscriber.RemoveWebhookEndpoint(endpoint);

            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<CannotRemoveLastEndpointFromSubscriberError>().Which.Message.Should().Contain(subscriber.Id);
        }

        [Fact, IsUnit]
        public void RemoveWebhookEndpoint_WhenEndpointDoesNotExists_ThenEndpointNotFoundInSubscriberErrorIsReturned()
        {
            var subscriber = new SubscriberEntity("testName", new EventEntity("eventName"));
            var endpoint = new EndpointEntity("uri", null, "POST", "*");
            subscriber.SetHooks(new WebhooksEntity(WebhooksEntityType.Webhooks, "$.Test", new[] { endpoint }));
            var newEndpoint = new EndpointEntity("new-uri", null, "POST", "sel");
            subscriber.SetWebhookEndpoint(newEndpoint);

            var result = subscriber.RemoveWebhookEndpoint(new EndpointEntity("uri", null, "POST", "custom"));

            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<EndpointNotFoundInSubscriberError>().Which.Message.Should().Contain(subscriber.Id);
        }

        [Fact, IsUnit]
        public void RemoveWebhookEndpoint_WhenListIsNull_ThenEndpointNotFoundInSubscriberErrorIsReturned()
        {
            var subscriber = new SubscriberEntity("testName", new EventEntity("eventName"));
            var endpoint = new EndpointEntity("uri", null, "POST", "*");

            var result = subscriber.RemoveWebhookEndpoint(endpoint);

            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<EndpointNotFoundInSubscriberError>().Which.Message.Should().Contain(subscriber.Id);
        }

        [Fact, IsUnit]
        public void SetCallbacksEndpoint_WhenSingleEndpointAdded_EndpointOnList()
        {
            var subscriber = new SubscriberEntity("testName", new EventEntity("eventName"));

            var newEndpoint = new EndpointEntity("new-uri", null, "POST", "*");
            SubscriberEntity result = subscriber.SetCallbackEndpoint(newEndpoint);

            result.Callbacks.Endpoints.Should().BeEquivalentTo(
                new[] { newEndpoint },
                options => options.IgnoringCyclicReferences());
        }

        [Fact, IsUnit]
        public void SetCallbacksEndpoint_WhenMultipleEndpointsAdded_EndpointsOnList()
        {
            var subscriber = new SubscriberEntity("testName", new EventEntity("eventName"));
            var endpoint = new EndpointEntity("uri", null, "POST", "*");
            subscriber.SetHooks(new WebhooksEntity(WebhooksEntityType.Callbacks, "$.Test", new[] { endpoint }));

            var newEndpoint = new EndpointEntity("new-uri", null, "POST", "sel");
            var result = subscriber.SetCallbackEndpoint(newEndpoint);

            result.Data.Callbacks.Endpoints.Should().BeEquivalentTo(
                new[] { endpoint, newEndpoint },
                options => options.IgnoringCyclicReferences());
        }

        [Fact, IsUnit]
        public void SetCallbacksEndpoint_WhenAlreadyExist_ThenUpdatedOnList()
        {
            var subscriber = new SubscriberEntity("testName", new EventEntity("eventName"));
            var endpoint = new EndpointEntity("uri", null, "POST", "*");
            subscriber.SetCallbackEndpoint(endpoint);

            var newEndpoint = new EndpointEntity("uri", null, "PUT", "*");
            var result = subscriber.SetCallbackEndpoint(newEndpoint);

            result.Data.Callbacks.Endpoints.Should().BeEquivalentTo(
                new[] { newEndpoint },
                options => options.IgnoringCyclicReferences());
        }

        [Fact, IsUnit]
        public void RemoveCallbacksEndpoint_WhenTwoEndpointsAreDefined_ThenOneIsRemoved()
        {
            var subscriber = new SubscriberEntity("testName", new EventEntity("eventName"));
            var endpoint = new EndpointEntity("uri", null, "POST", "*");
            subscriber.SetHooks(new WebhooksEntity(WebhooksEntityType.Callbacks, "$.Test", new[] { endpoint }, null));
            var newEndpoint = new EndpointEntity("new-uri", null, "POST", "sel");
            subscriber.SetCallbackEndpoint(newEndpoint);

            var result = subscriber.RemoveCallbackEndpoint(newEndpoint);

            result.Data.Callbacks.Endpoints.Should().BeEquivalentTo(
                new[] { endpoint },
                options => options.IgnoringCyclicReferences());
        }

        [Fact, IsUnit]
        public void RemoveCallbacksEndpoint_WhenOnlyOneEndpointDefined_ThenIsRemoved()
        {
            var subscriber = new SubscriberEntity("testName", new EventEntity("eventName"));
            var endpoint = new EndpointEntity("uri", null, "POST", "*");
            subscriber.SetCallbackEndpoint(endpoint);

            var result = subscriber.RemoveCallbackEndpoint(endpoint);

            result.Data.Callbacks.Endpoints.Should().BeEmpty();
        }

        [Fact, IsUnit]
        public void RemoveCallbacksEndpoint_WhenEndpointDoesNotExists_ThenEndpointNotFoundInSubscriberErrorIsReturned()
        {
            var subscriber = new SubscriberEntity("testName", new EventEntity("eventName"));
            var endpoint = new EndpointEntity("uri", null, "POST", "*");
            subscriber.SetHooks(new WebhooksEntity(WebhooksEntityType.Callbacks, "$.Test", new[] { endpoint }));
            var newEndpoint = new EndpointEntity("new-uri", null, "POST", "sel");
            subscriber.SetCallbackEndpoint(newEndpoint);

            var result = subscriber.RemoveCallbackEndpoint(new EndpointEntity("uri", null, "POST", "custom"));

            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<EndpointNotFoundInSubscriberError>();
        }

        [Fact, IsUnit]
        public void RemoveCallbacksEndpoint_WhenListIsNull_ThenEndpointNotFoundInSubscriberErrorIsReturned()
        {
            var subscriber = new SubscriberEntity("testName", new EventEntity("eventName"));
            var endpoint = new EndpointEntity("uri", null, "POST", "*");

            var result = subscriber.RemoveCallbackEndpoint(endpoint);

            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<EndpointNotFoundInSubscriberError>();
        }

        [Fact, IsUnit]
        public void SetDlqEndpoint_WhenSingleEndpointAdded_EndpointOnList()
        {
            var subscriber = new SubscriberEntity("testName", new EventEntity("eventName"));

            var newEndpoint = new EndpointEntity("new-uri", null, "POST", "*");
            SubscriberEntity result = subscriber.SetDlqEndpoint(newEndpoint);

            result.DlqHooks.Endpoints.Should().BeEquivalentTo(
                new[] { newEndpoint },
                options => options.IgnoringCyclicReferences());
        }

        [Fact, IsUnit]
        public void SetDlqEndpoint_WhenMultipleEndpointsAdded_EndpointsOnList()
        {
            var subscriber = new SubscriberEntity("testName", new EventEntity("eventName"));
            var endpoint = new EndpointEntity("uri", null, "POST", "*");
            subscriber.SetHooks(new WebhooksEntity(WebhooksEntityType.DlqHooks, "$.Test", new[] { endpoint }));

            var newEndpoint = new EndpointEntity("new-uri", null, "POST", "sel");
            var result = subscriber.SetDlqEndpoint(newEndpoint);

            result.Data.DlqHooks.Endpoints.Should().BeEquivalentTo(
                new[] { endpoint, newEndpoint },
                options => options.IgnoringCyclicReferences());
        }

        [Fact, IsUnit]
        public void SetDlqEndpoint_WhenAlreadyExist_ThenUpdatedOnList()
        {
            var subscriber = new SubscriberEntity("testName", new EventEntity("eventName"));
            var endpoint = new EndpointEntity("uri", null, "POST", "*");
            subscriber.SetDlqEndpoint(endpoint);

            var newEndpoint = new EndpointEntity("uri", null, "PUT", "*");
            var result = subscriber.SetDlqEndpoint(newEndpoint);

            result.Data.DlqHooks.Endpoints.Should().BeEquivalentTo(
                new[] { newEndpoint },
                options => options.IgnoringCyclicReferences());
        }

        [Fact, IsUnit]
        public void RemoveDlqEndpoint_WhenTwoEndpointsAreDefined_ThenOneIsRemoved()
        {
            var subscriber = new SubscriberEntity("testName", new EventEntity("eventName"));
            var endpoint = new EndpointEntity("uri", null, "POST", "*");
            subscriber.SetHooks(new WebhooksEntity(WebhooksEntityType.DlqHooks, "$.Test", new[] { endpoint }, null));
            var newEndpoint = new EndpointEntity("new-uri", null, "POST", "sel");
            subscriber.SetDlqEndpoint(newEndpoint);

            var result = subscriber.RemoveDlqEndpoint(newEndpoint);

            result.Data.DlqHooks.Endpoints.Should().BeEquivalentTo(
                new[] { endpoint },
                options => options.IgnoringCyclicReferences());
        }

        [Fact, IsUnit]
        public void RemoveDlqEndpoint_WhenOnlyOneEndpointDefined_ThenIsRemoved()
        {
            var subscriber = new SubscriberEntity("testName", new EventEntity("eventName"));
            var endpoint = new EndpointEntity("uri", null, "POST", "*");
            subscriber.SetDlqEndpoint(endpoint);

            var result = subscriber.RemoveDlqEndpoint(endpoint);

            result.Data.DlqHooks.Endpoints.Should().BeEmpty();
        }

        [Fact, IsUnit]
        public void RemoveDlqEndpoint_WhenEndpointDoesNotExists_ThenEndpointNotFoundInSubscriberErrorIsReturned()
        {
            var subscriber = new SubscriberEntity("testName", new EventEntity("eventName"));
            var endpoint = new EndpointEntity("uri", null, "POST", "*");
            subscriber.SetHooks(new WebhooksEntity(WebhooksEntityType.DlqHooks, "$.Test", new[] { endpoint }));
            var newEndpoint = new EndpointEntity("new-uri", null, "POST", "sel");
            subscriber.SetDlqEndpoint(newEndpoint);

            var result = subscriber.RemoveDlqEndpoint(new EndpointEntity("uri", null, "POST", "custom"));

            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<EndpointNotFoundInSubscriberError>();
        }

        [Fact, IsUnit]
        public void RemoveDlqEndpoint_WhenListIsNull_ThenEndpointNotFoundInSubscriberErrorIsReturned()
        {
            var subscriber = new SubscriberEntity("testName", new EventEntity("eventName"));
            var endpoint = new EndpointEntity("uri", null, "POST", "*");

            var result = subscriber.RemoveDlqEndpoint(endpoint);

            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<EndpointNotFoundInSubscriberError>();
        }
    }
}