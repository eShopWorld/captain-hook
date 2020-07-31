using CaptainHook.Domain.Entities;
using CaptainHook.Domain.ValueObjects;
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

        private readonly SubscriberRepository _repository;

        public SubscriberRepositoryTests()
        {
            _cosmosDbRepositoryMock = new Mock<ICosmosDbRepository>();
            _queryBuilderMock = new Mock<ISubscriberQueryBuilder>();

            _repository = new SubscriberRepository(_cosmosDbRepositoryMock.Object, _queryBuilderMock.Object);
        }

        [Theory, IsUnit]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task GetSubscribersList_InvalidEventNameThrowsException(string eventName)
        {
            // Act
            Task act() => _repository.GetSubscribersListAsync(eventName);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(act);
        }

        [Fact, IsUnit]
        public async Task GetSubscribersListBuildsQuery()
        {
            // Arrange
            var eventName = "eventName";

            // Act
            var result = await _repository.GetSubscribersListAsync(eventName);

            // Assert
            _queryBuilderMock.Verify(x => x.BuildSelectSubscribersList(eventName));
        }

        [Fact, IsUnit]
        public async Task GetSubscribersListInvokesQuery()
        {
            // Arrange
            var eventName = "eventName";

            // Act
            var result = await _repository.GetSubscribersListAsync(eventName);

            // Assert
            _cosmosDbRepositoryMock.Verify(x => x.QueryAsync<SubscriberDocument>(It.IsAny<CosmosQuery>()));
        }

        [Fact, IsUnit]
        public async Task GetSubscribersListReturnsCorrectModels()
        {
            // Arrange
            var eventName = "eventName";
            var endpoint = new EndpointSubdocument
            {
                HttpVerb = "POST",
                Uri = "http://test",
                EndpointSelector = "selector",
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
            var sampleDocument = new SubscriberDocument()
            {
                SubscriberName = "subscriberName",
                EventName = eventName,
                WebhookType = WebhookType.Webhook,
                WebhookSelectionRule = "rule",
                Endpoints = new EndpointSubdocument[] { endpoint }
            };
            _cosmosDbRepositoryMock
                .Setup(x => x.QueryAsync<SubscriberDocument>(It.IsAny<CosmosQuery>()))
                .ReturnsAsync(new List<SubscriberDocument> { sampleDocument });

            var expectedSubscriberEntity = new SubscriberEntity(sampleDocument.SubscriberName, sampleDocument.WebhookSelectionRule, new EventEntity(eventName));
            var sampleAuthStoreEntity = new SecretStoreEntity(endpoint.Authentication.KeyVaultName, endpoint.Authentication.SecretName);
            var expectedAuthenticationEntity = new AuthenticationEntity(endpoint.Authentication.ClientId, sampleAuthStoreEntity, endpoint.Authentication.Uri, endpoint.Authentication.Type, endpoint.Authentication.Scopes);
            var expectedEndpointEntity = new EndpointEntity(endpoint.Uri, expectedAuthenticationEntity, endpoint.HttpVerb, endpoint.EndpointSelector);
            expectedSubscriberEntity.AddWebhookEndpoint(expectedEndpointEntity);

            // Act
            var result = await _repository.GetSubscribersListAsync(eventName);

            // Assert
            result.Data.Should().BeEquivalentTo(new [] { expectedSubscriberEntity }, options => options.IgnoringCyclicReferences());
        }

        [Fact, IsUnit]
        public async Task GetSubscribersListAsync_should_map_SubscriberDocument_to_SubscriberEntity_correctly()
        {
            // Arrange
            const string eventName = "eventName";
            var response = new[]
            {
                new SubscriberDocument
                {
                    SubscriberName = "subscriberName",
                    EventName = eventName,
                    WebhookType = WebhookType.Webhook,
                    WebhookSelectionRule = "rule",
                    Endpoints = new EndpointSubdocument[]
                    {
                        new EndpointSubdocument
                        {
                            HttpVerb = "POST",
                            Uri = "http://test",
                            EndpointSelector = "selector",
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
                        new EndpointSubdocument
                        {
                            HttpVerb = "GET",
                            Uri = "http://test2",
                            EndpointSelector = "selector2",
                            Authentication = new AuthenticationData
                            {
                                ClientId = "clientid",
                                KeyVaultName = "keyvaultname",
                                Scopes = new[] { "scope" },
                                SecretName = "secret",
                                Type = "type",
                                Uri = "uri"
                            }
                        }
                    }
                },
            };

            _cosmosDbRepositoryMock
                .Setup(x => x.QueryAsync<SubscriberDocument>(It.IsAny<CosmosQuery>()))
                .ReturnsAsync(response);

            // Act
            var result = await _repository.GetSubscribersListAsync(eventName);

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

            result.Data.Should().BeEquivalentTo(expected, options => options.IgnoringCyclicReferences());
        }

        [Fact, IsUnit]
        public async Task GetSubscriber_InvalidSubscriberIdThrowsException()
        {
            // Act
            Task act() => _repository.GetSubscriberAsync(null);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(act);
        }

        [Fact, IsUnit]
        public async Task GetSubscriberBuildsQuery()
        {
            // Arrange
            var subscriberId = new SubscriberId("eventName", "subscriberName");

            // Act
            var result = await _repository.GetSubscriberAsync(subscriberId);

            // Assert
            _queryBuilderMock.Verify(x => x.BuildSelectSubscriber(subscriberId));
        }

        [Fact, IsUnit]
        public async Task GetSubscriberInvokesQuery()
        {
            // Arrange
            var subscriberId = new SubscriberId("eventName", "subscriberName");

            // Act
            var result = await _repository.GetSubscriberAsync(subscriberId);

            // Assert
            _cosmosDbRepositoryMock.Verify(x => x.QueryAsync<SubscriberDocument>(It.IsAny<CosmosQuery>()));
        }

        [Fact, IsUnit]
        public async Task GetSubscriberReturnsCorrectModel()
        {
            // Arrange
            var eventName = "eventName";
            var endpoint = new EndpointSubdocument
            {
                HttpVerb = "POST",
                Uri = "http://test",
                EndpointSelector = "selector",
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
            var sampleDocument = new SubscriberDocument()
            {
                SubscriberName = "subscriberName",
                EventName = eventName,
                WebhookType = WebhookType.Webhook,
                WebhookSelectionRule = "rule",
                Endpoints = new EndpointSubdocument[] { endpoint }
            };
            _cosmosDbRepositoryMock
                .Setup(x => x.QueryAsync<SubscriberDocument>(It.IsAny<CosmosQuery>()))
                .ReturnsAsync(new List<SubscriberDocument> { sampleDocument });

            var expectedSubscriberEntity = new SubscriberEntity(sampleDocument.SubscriberName, sampleDocument.WebhookSelectionRule, new EventEntity(eventName));
            var sampleAuthStoreEntity = new SecretStoreEntity(endpoint.Authentication.KeyVaultName, endpoint.Authentication.SecretName);
            var expectedAuthenticationEntity = new AuthenticationEntity(endpoint.Authentication.ClientId, sampleAuthStoreEntity, endpoint.Authentication.Uri, endpoint.Authentication.Type, endpoint.Authentication.Scopes);
            var expectedEndpointEntity = new EndpointEntity(endpoint.Uri, expectedAuthenticationEntity, endpoint.HttpVerb, endpoint.EndpointSelector);
            expectedSubscriberEntity.AddWebhookEndpoint(expectedEndpointEntity);

            // Act
            var result = await _repository.GetSubscriberAsync(new SubscriberId("eventName", "subscriberName"));

            // Assert
            result.Data.Should().BeEquivalentTo(expectedSubscriberEntity, options => options.IgnoringCyclicReferences());
        }

        [Fact, IsUnit]
        public async Task GetSubscriberAsync_should_map_SubscriberDocument_to_SubscriberEntity_correctly()
        {
            // Arrange
            const string eventName = "eventName";
            var response = new[]
            {
                new SubscriberDocument
                {
                    SubscriberName = "subscriberName",
                    EventName = eventName,
                    WebhookType = WebhookType.Webhook,
                    WebhookSelectionRule = "rule",
                    Endpoints = new EndpointSubdocument[]
                    {
                        new EndpointSubdocument
                        {
                            HttpVerb = "POST",
                            Uri = "http://test",
                            EndpointSelector = "selector",
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
                        new EndpointSubdocument
                        {
                            HttpVerb = "GET",
                            Uri = "http://test2",
                            EndpointSelector = "selector2",
                            Authentication = new AuthenticationData
                            {
                                ClientId = "clientid",
                                KeyVaultName = "keyvaultname",
                                Scopes = new[] { "scope" },
                                SecretName = "secret",
                                Type = "type",
                                Uri = "uri"
                            }
                        }
                    }
                },
            };

            _cosmosDbRepositoryMock
                .Setup(x => x.QueryAsync<SubscriberDocument>(It.IsAny<CosmosQuery>()))
                .ReturnsAsync(response);

            // Act
            var result = await _repository.GetSubscriberAsync(new SubscriberId(eventName, "subscriberName"));

            // Assert
            var expected = new SubscriberEntity("subscriberName", "rule", new EventEntity(eventName))
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
                    "selector2"));

            result.Data.Should().BeEquivalentTo(expected, options => options.IgnoringCyclicReferences());
        }

        [Fact, IsUnit]
        public async Task AddSubscriber_InvalidSubscriberIdThrowsException()
        {
            // Act
            Task act() => _repository.AddSubscriberAsync(null);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(act);
        }

        [Fact, IsUnit]
        public async Task AddSubscriberInvokesCreate()
        {
            // Arrange
            var subscriberDocument = new SubscriberDocument
            {
                SubscriberName = "subscriberName",
                EventName = "eventName",
                WebhookType = WebhookType.Webhook,
                WebhookSelectionRule = "rule",
                Endpoints = new EndpointSubdocument[] { }
            };
            var subscriber = new SubscriberEntity("subscriberName", "rule", new EventEntity("eventName"));
            _cosmosDbRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<SubscriberDocument>()))
                .ReturnsAsync(new DocumentContainer<SubscriberDocument>(subscriberDocument,"etag"));

            // Act
            var result = await _repository.AddSubscriberAsync(subscriber);

            // Assert
            _cosmosDbRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<SubscriberDocument>()));
        }
    }
}
