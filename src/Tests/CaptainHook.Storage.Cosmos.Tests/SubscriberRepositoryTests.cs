using CaptainHook.Domain.Entities;
using CaptainHook.Storage.Cosmos.Models;
using CaptainHook.Storage.Cosmos.QueryBuilders;
using Eshopworld.Data.CosmosDb;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace CaptainHook.Storage.Cosmos.Tests
{
    public class SubscriberRepositoryTests
    {
        private readonly Mock<ICosmosDbRepository> _cosmosDbRepositoryMock;
        private readonly Mock<ISubscriberQueryBuilder> _queryBuilderMock;

        private readonly SubscriberRepository repository;

        public SubscriberRepositoryTests()
        {
            _cosmosDbRepositoryMock = new Mock<ICosmosDbRepository>();
            _queryBuilderMock = new Mock<ISubscriberQueryBuilder>();

            repository = new SubscriberRepository(_cosmosDbRepositoryMock.Object, _queryBuilderMock.Object);
        }

        [Theory, IsUnit]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task InvalidEventNameThrowsException(string eventName)
        {
            // Act
            Task act() => repository.GetSubscribersListAsync(eventName);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(act);
        }

        [Fact, IsUnit]
        public async Task GetSubscribersListBuildsQuery()
        {
            // Arrange
            var eventName = "eventName";

            // Act
            var result = await repository.GetSubscribersListAsync(eventName);

            // Assert
            _queryBuilderMock.Verify(x => x.BuildSelectSubscribersListEndpoints(eventName));
        }

        [Fact, IsUnit]
        public async Task GetSubscribersListInvokesQuery()
        {
            // Arrange
            var eventName = "eventName";

            // Act
            var result = await repository.GetSubscribersListAsync(eventName);

            // Assert
            _cosmosDbRepositoryMock.Verify(x => x.QueryAsync<EndpointDocument>(It.IsAny<CosmosQuery>()));
        }

        [Fact, IsUnit]
        public async Task GetSubscribersListReturnsCorrectModels()
        {
            // Arrange
            var eventName = "eventName";
            var sampleDocument = new EndpointDocument()
            {
                SubscriberName = "subscriberName",
                EventName = eventName,
                HttpVerb = "POST",
                Uri = "http://test",
                EndpointSelector = "selector",
                WebhookType = WebhookType.Webhook,
                WebhookSelectionRule = "rule",
                Authentication = new AuthenticationData
                {
                    ClientId = "clientid",
                    KeyVaultName = "keyvaultname",
                    Scopes = new string[] { "scope" },
                    SecretName = "secret",
                    Type = "type",
                    Uri = "uri"
                }
            };
            _cosmosDbRepositoryMock
                .Setup(x => x.QueryAsync<EndpointDocument>(It.IsAny<CosmosQuery>()))
                .ReturnsAsync(new List<EndpointDocument> { sampleDocument });

            var expectedSubscriberEntity = new SubscriberEntity(sampleDocument.SubscriberName, sampleDocument.WebhookSelectionRule, new EventEntity(eventName));
            var sampleAuthStoreEntity = new SecretStoreEntity(sampleDocument.Authentication.KeyVaultName, sampleDocument.Authentication.SecretName);
            var expectedAuthenticationEntity = new AuthenticationEntity(sampleDocument.Authentication.ClientId, sampleAuthStoreEntity, sampleDocument.Authentication.Uri, sampleDocument.Authentication.Type, sampleDocument.Authentication.Scopes);
            var expectedEndpointEntity = new EndpointEntity(sampleDocument.Uri, expectedAuthenticationEntity, sampleDocument.HttpVerb, sampleDocument.EndpointSelector);
            expectedSubscriberEntity.AddWebhookEndpoint(expectedEndpointEntity);

            // Act
            var result = await repository.GetSubscribersListAsync(eventName);

            // Assert
            result.Should().BeEquivalentTo(new [] { expectedSubscriberEntity }, options => options.IgnoringCyclicReferences());
        }

        [Fact, IsUnit]
        public async Task TestMethodName()
        {
            // Arrange
            const string eventName = "eventName";
            var response = new[]
            {
                new EndpointDocument
                {
                    SubscriberName = "subscriberName",
                    EventName = eventName,
                    HttpVerb = "POST",
                    Uri = "http://test",
                    EndpointSelector = "selector",
                    WebhookType = WebhookType.Webhook,
                    WebhookSelectionRule = "rule",
                    Authentication = new AuthenticationData
                    {
                        ClientId = "clientid",
                        KeyVaultName = "keyvaultname",
                        Scopes = new[] { "scope" },
                        SecretName = "secret",
                        Type = "type",
                        Uri = "uri"
                    }
                },
                new EndpointDocument
                {
                    SubscriberName = "subscriberName",
                    EventName = eventName,
                    HttpVerb = "GET",
                    Uri = "http://test2",
                    EndpointSelector = "selector2",
                    WebhookType = WebhookType.Webhook,
                    WebhookSelectionRule = "rule",
                    Authentication = new AuthenticationData
                    {
                        ClientId = "clientid",
                        KeyVaultName = "keyvaultname",
                        Scopes = new[] { "scope" },
                        SecretName = "secret",
                        Type = "type",
                        Uri = "uri"
                    }
                },
            };

            _cosmosDbRepositoryMock
                .Setup(x => x.QueryAsync<EndpointDocument>(It.IsAny<CosmosQuery>()))
                .ReturnsAsync(response);

            // Act
            var result = await repository.GetSubscribersListAsync(eventName);

            // Assert
            var expected = new[]
            {
                new SubscriberEntity("subscriberName", "rule", new EventEntity(eventName))
                    .AddWebhookEndpoint(new EndpointEntity(
                        "http://test",
                        new AuthenticationEntity(
                            "clientid",
                            new SecretStoreEntity("keyvaultname", "secret"),
                            "uri",
                            "type",
                            new[] { "scope" }),
                        "POST",
                        "selector"))
                    .AddWebhookEndpoint(
                        new EndpointEntity("http://test2",
                        new AuthenticationEntity(
                            "clientid",
                            new SecretStoreEntity("keyvaultname", "secret"),
                            "uri",
                            "type",
                            new[] { "scope" }),
                        "GET",
                        "selector2"))
            };
            result.Should().BeEquivalentTo(expected, options => options.IgnoringCyclicReferences());
        }
    }
}
