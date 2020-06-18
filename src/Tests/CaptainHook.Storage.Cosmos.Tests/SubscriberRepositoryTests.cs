using CaptainHook.Domain.Models;
using CaptainHook.Repository.Models;
using CaptainHook.Repository.QueryBuilders;
using Eshopworld.Data.CosmosDb;
using Eshopworld.Tests.Core;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Xunit;

namespace CaptainHook.Repository.Tests
{
    public class SubscriberRepositoryTests
    {
        private readonly Mock<ICosmosDbRepository> cosmosDbRepositoryMock;
        private readonly Mock<ISubscriberQueryBuilder> queryBuilderMock;

        private readonly SubscriberRepository repository;

        public SubscriberRepositoryTests()
        {
            cosmosDbRepositoryMock = new Mock<ICosmosDbRepository>();
            queryBuilderMock = new Mock<ISubscriberQueryBuilder>();

            repository = new SubscriberRepository(cosmosDbRepositoryMock.Object, queryBuilderMock.Object);
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
            queryBuilderMock.Verify(x => x.BuildSelectSubscribersListEndpoints(eventName));
        }

        [Fact, IsUnit]
        public async Task GetSubscribersListInvokesQuery()
        {
            // Arrange
            var eventName = "eventName";

            // Act
            var result = await repository.GetSubscribersListAsync(eventName);

            // Assert
            cosmosDbRepositoryMock.Verify(x => x.QueryAsync<EndpointDocument>(It.IsAny<CosmosQuery>()));
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
            cosmosDbRepositoryMock
                .Setup(x => x.QueryAsync<EndpointDocument>(It.IsAny<CosmosQuery>()))
                .ReturnsAsync(endpointDocuments);

            var expectedSubscriberModel = new SubscriberModel(sampleDocument.SubscriberName, sampleDocument.WebhookSelectionRule);
            var sampleAuthStoreModel = new SecretStoreModel(sampleDocument.Authentication.KeyVaultName, sampleDocument.Authentication.SecretName);
            var expectedAuthenticationModel = new AuthenticationModel(sampleDocument.Authentication.ClientId, sampleAuthStoreModel, sampleDocument.Authentication.Uri, sampleDocument.Authentication.Type, sampleDocument.Authentication.Scopes);
            var expectedEndpointModel = new EndpointModel(sampleDocument.Uri, expectedAuthenticationModel, sampleDocument.HttpVerb, sampleDocument.EndpointSelector);
            expectedSubscriberModel.AddWebhookEndpoint(expectedEndpointModel);

            // Act
            var result = await repository.GetSubscribersListAsync(eventName);

            // Assert
            using (new AssertionScope())
            {
                result.Should().HaveCount(1);

                result.First().Should().BeEquivalentTo(expectedSubscriberModel, 
                    options => options
                        .Excluding(x => x.SelectedMemberPath == "Webhooks.Endpoints[0].ParentSubscriber"));
            }
        }
    }
}
