using CaptainHook.Domain.Entities;
using CaptainHook.Storage.Cosmos;
using CaptainHook.Storage.Cosmos.Models;
using CaptainHook.Storage.Cosmos.QueryBuilders;
using Eshopworld.Data.CosmosDb;
using Eshopworld.Tests.Core;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
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

        [Fact, IsUnit]
        public async Task EmptyEventNameThrowsException()
        {
            // Arrange
            var eventName = "";

            // Act
            Task act() => repository.GetSubscribersListAsync(eventName);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(act);
        }

        [Fact, IsUnit]
        public async Task BlankEventNameThrowsException()
        {
            // Arrange
            var eventName = " ";

            // Act
            Task act() => repository.GetSubscribersListAsync(eventName);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(act);
        }

        [Fact, IsUnit]
        public async Task NullEventNameThrowsException()
        {
            // Arrange
            string eventName = null;

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
            var endpointDocuments = new List<EndpointDocument>() { sampleDocument };
            _cosmosDbRepositoryMock
                .Setup(x => x.QueryAsync<EndpointDocument>(It.IsAny<CosmosQuery>()))
                .ReturnsAsync(endpointDocuments);

            var expectedSubscriberEntity = new SubscriberEntity(sampleDocument.SubscriberName, sampleDocument.WebhookSelectionRule);
            var sampleAuthStoreEntity = new SecretStoreEntity(sampleDocument.Authentication.KeyVaultName, sampleDocument.Authentication.SecretName);
            var expectedAuthenticationEntity = new AuthenticationEntity(sampleDocument.Authentication.ClientId, sampleAuthStoreEntity, sampleDocument.Authentication.Uri, sampleDocument.Authentication.Type, sampleDocument.Authentication.Scopes);
            var expectedEndpointEntity = new EndpointEntity(sampleDocument.Uri, expectedAuthenticationEntity, sampleDocument.HttpVerb, sampleDocument.EndpointSelector);
            expectedSubscriberEntity.AddWebhookEndpoint(expectedEndpointEntity);

            // Act
            var result = await repository.GetSubscribersListAsync(eventName);

            // Assert
            using (new AssertionScope())
            {
                result.Should().HaveCount(1);

                result.First().Should().BeEquivalentTo(expectedSubscriberEntity, 
                    options => options
                        .Excluding(x => x.SelectedMemberPath == "Webhooks.Endpoints[0].ParentSubscriber"));
            }
        }
    }
}
