using System;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Errors;
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


        //TODO:
        // tests for removing webhook
        // tests for callback operations


        //[Fact, IsUnit]
        //public void RemoveCallbackEndpoint_WhenOnlyOneEndpointExists_ThenCannotRemoveLastEndpointFromSubscriberErrorIsReturned()
        //{
        //    var subscriber = new SubscriberEntity("testName");
        //    var endpoint = new EndpointEntity("uri", null, "POST", "*");
        //    subscriber.SetCallbackEndpoint(endpoint);

        //    var result = subscriber.RemoveCallbackEndpoint(endpoint);
        //    result.IsError.Should().BeTrue();
        //    result.Error.Should().BeOfType<CannotRemoveLastEndpointFromSubscriberError>();
        //}



        [Fact, IsUnit]
        public void SetDlqEndpoint_WhenSettingFirstEndpoint_ThenAdded()
        {
            var subscriber = new SubscriberEntity("testName");

            var newEndpoint = new EndpointEntity("new-uri", null, "POST", "*");
            SubscriberEntity result = subscriber.SetDlqEndpoint(newEndpoint);

            result.Dlq.Endpoints.Should().BeEquivalentTo(
                new[] { newEndpoint },
                options => options.IgnoringCyclicReferences());
        }

        [Fact, IsUnit]
        public void SetDlqEndpoint_WhenSettingNotFirstEndpoint_ThenAdded()
        {
            var subscriber = new SubscriberEntity("testName");
            var endpoint = new EndpointEntity("uri", null, "POST", "*");
            subscriber.AddDlq(new WebhooksEntity("$.Test", new[] { endpoint }));

            var newEndpoint = new EndpointEntity("new-uri", null, "POST", "sel");
            var result = subscriber.SetDlqEndpoint(newEndpoint);

            result.Data.Dlq.Endpoints.Should().BeEquivalentTo(
                new[] { endpoint, newEndpoint },
                options => options.IgnoringCyclicReferences());
        }

        [Fact, IsUnit]
        public void SetDlqEndpoint_WhenAlreadyExist_ThenUpdated()
        {
            var subscriber = new SubscriberEntity("testName");
            var endpoint = new EndpointEntity("uri", null, "POST", "*");
            subscriber.SetDlqEndpoint(endpoint);

            var newEndpoint = new EndpointEntity("uri", null, "PUT", "*");
            var result = subscriber.SetDlqEndpoint(newEndpoint);

            result.Data.Dlq.Endpoints.Should().BeEquivalentTo(
                new[] { newEndpoint },
                options => options.IgnoringCyclicReferences());
        }


        [Fact, IsUnit]
        public void RemoveDlqEndpoint_WhenTwoEndpointsAreDefined_ThenOneIsRemoved()
        {
            var subscriber = new SubscriberEntity("testName");
            var endpoint = new EndpointEntity("uri", null, "POST", "*");
            subscriber.AddDlq(new WebhooksEntity("$.Test", new[] { endpoint }));
            var newEndpoint = new EndpointEntity("new-uri", null, "POST", "sel");
            subscriber.SetDlqEndpoint(newEndpoint);

            var result = subscriber.RemoveDlqEndpoint(newEndpoint);

            result.Data.Dlq.Endpoints.Should().BeEquivalentTo(
                new[] { endpoint },
                options => options.IgnoringCyclicReferences());
        }

        [Fact, IsUnit]
        public void RemoveDlqEndpoint_WhenOnlyOneEndpointDefined_ThenIsRemoved()
        {
            var subscriber = new SubscriberEntity("testName");
            var endpoint = new EndpointEntity("uri", null, "POST", "*");
            subscriber.SetDlqEndpoint(endpoint);

            var result = subscriber.RemoveDlqEndpoint(endpoint);

            result.Data.Dlq.Endpoints.Should().BeEmpty();
        }

        [Fact, IsUnit]
        public void RemoveDlqEndpoint_WhenEndpointDoesNotExists_ThenEndpointNotFoundInSubscriberErrorIsReturned()
        {
            var subscriber = new SubscriberEntity("testName");
            var endpoint = new EndpointEntity("uri", null, "POST", "*");
            subscriber.AddDlq(new WebhooksEntity("$.Test", new[] { endpoint }));
            var newEndpoint = new EndpointEntity("new-uri", null, "POST", "sel");
            subscriber.SetDlqEndpoint(newEndpoint);

            var result = subscriber.RemoveDlqEndpoint(new EndpointEntity("uri", null, "POST", "custom"));

            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<EndpointNotFoundInSubscriberError>();
        }

        [Fact, IsUnit]
        public void RemoveDlqEndpoint_WhenListIsNull_ThenEndpointNotFoundInSubscriberErrorIsReturned()
        {
            var subscriber = new SubscriberEntity("testName");
            var endpoint = new EndpointEntity("uri", null, "POST", "*");

            var result = subscriber.RemoveDlqEndpoint(endpoint);

            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<EndpointNotFoundInSubscriberError>();
        }
    }
}