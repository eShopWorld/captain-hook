using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Errors;
using CaptainHook.Domain.ValueObjects;
using CaptainHook.Storage.Cosmos.Models;
using CaptainHook.Storage.Cosmos.QueryBuilders;
using Eshopworld.Data.CosmosDb;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
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
        public async Task GetSubscribersListAsync_should_throw_for_invalid_EventName(string eventName)
        {
            // Act
            Task act() => _repository.GetSubscribersListAsync(eventName);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(act);
        }        

        [Fact, IsUnit]
        public async Task GetSubscribersListAsync_should_invoke_BuildSelectForEventSubscribers()
        {
            // Arrange
            var eventName = "eventName";

            // Act
            var result = await _repository.GetSubscribersListAsync(eventName);

            // Assert
            _cosmosDbRepositoryMock.Verify(x => x.QueryAsync<SubscriberDocument>(It.IsAny<CosmosQuery>()));
        }

        [Fact, IsUnit]
        public async Task GetSubscribersListAsync_shoudl_return_correct_models()
        {
            // Arrange
            var eventName = "eventName";
            var endpoint = new EndpointSubdocument
            {
                HttpVerb = "POST",
                Uri = "http://test",
                Selector = "selector",
                Type = EndpointType.Webhook,
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
                WebhookSelectionRule = "rule",
                Endpoints = new[] { endpoint }
            };
            _cosmosDbRepositoryMock
                .Setup(x => x.QueryAsync<SubscriberDocument>(It.IsAny<CosmosQuery>()))
                .ReturnsAsync(new List<SubscriberDocument> { sampleDocument });

            var expectedSubscriberEntity = new SubscriberEntity(sampleDocument.SubscriberName, sampleDocument.WebhookSelectionRule, new EventEntity(eventName));
            var sampleAuthStoreEntity = new SecretStoreEntity(endpoint.Authentication.KeyVaultName, endpoint.Authentication.SecretName);
            var expectedAuthenticationEntity = new AuthenticationEntity(endpoint.Authentication.ClientId, sampleAuthStoreEntity, endpoint.Authentication.Uri, endpoint.Authentication.Type, endpoint.Authentication.Scopes);
            var expectedEndpointEntity = new EndpointEntity(endpoint.Uri, expectedAuthenticationEntity, endpoint.HttpVerb, endpoint.Selector);
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
                    WebhookSelectionRule = "rule",
                    Endpoints = new[]
                    {
                        new EndpointSubdocument
                        {
                            HttpVerb = "POST",
                            Uri = "http://test",
                            Selector = "selector",
                            Type = EndpointType.Webhook,
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
                            Selector = "selector2",
                            Type = EndpointType.Webhook,
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
        public async Task GetSubscriber_shoudl_throw_for_invalid_SubscriberId()
        {
            // Act
            Task act() => _repository.GetSubscriberAsync(null);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(act);
        }

        [Fact, IsUnit]
        public async Task GetSubscriberAsync_should_invoke_BuildSelectSubscriber()
        {
            // Arrange
            var subscriberId = new SubscriberId("eventName", "subscriberName");

            // Act
            var result = await _repository.GetSubscriberAsync(subscriberId);

            // Assert
            _queryBuilderMock.Verify(x => x.BuildSelectSubscriber(subscriberId));
        }

        [Fact, IsUnit]
        public async Task GetSubscriberAsync_should_invoke_Query()
        {
            // Arrange
            var subscriberId = new SubscriberId("eventName", "subscriberName");

            // Act
            var result = await _repository.GetSubscriberAsync(subscriberId);

            // Assert
            _cosmosDbRepositoryMock.Verify(x => x.QueryAsync<SubscriberDocument>(It.IsAny<CosmosQuery>()));
        }

        [Fact, IsUnit]
        public async Task GetSubscriberAsync_should_return_correct_model()
        {
            // Arrange
            var eventName = "eventName";
            var endpoint = new EndpointSubdocument
            {
                HttpVerb = "POST",
                Uri = "http://test",
                Selector = "selector",
                Type = EndpointType.Webhook,
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
                WebhookSelectionRule = "rule",
                Endpoints = new[] { endpoint }
            };
            _cosmosDbRepositoryMock
                .Setup(x => x.QueryAsync<SubscriberDocument>(It.IsAny<CosmosQuery>()))
                .ReturnsAsync(new List<SubscriberDocument> { sampleDocument });

            var expectedSubscriberEntity = new SubscriberEntity(sampleDocument.SubscriberName, sampleDocument.WebhookSelectionRule, new EventEntity(eventName));
            var sampleAuthStoreEntity = new SecretStoreEntity(endpoint.Authentication.KeyVaultName, endpoint.Authentication.SecretName);
            var expectedAuthenticationEntity = new AuthenticationEntity(endpoint.Authentication.ClientId, sampleAuthStoreEntity, endpoint.Authentication.Uri, endpoint.Authentication.Type, endpoint.Authentication.Scopes);
            var expectedEndpointEntity = new EndpointEntity(endpoint.Uri, expectedAuthenticationEntity, endpoint.HttpVerb, endpoint.Selector);
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
                    WebhookSelectionRule = "rule",
                    Endpoints = new[]
                    {
                        new EndpointSubdocument
                        {
                            HttpVerb = "POST",
                            Uri = "http://test",
                            Selector = "selector",
                            Type = EndpointType.Webhook,
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
                            Selector = "selector2",
                            Type = EndpointType.Webhook,
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
        public async Task AddSubscriberAsync_should_throw_for_invalid_SubscriberId()
        {
            // Act
            Task act() => _repository.AddSubscriberAsync(null);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(act);
        }

        [Fact, IsUnit]
        public async Task AddSubscriberAsync_should_invoke_CreateAsync()
        {
            // Arrange
            var subscriberDocument = new SubscriberDocument
            {
                SubscriberName = "subscriberName",
                EventName = "eventName",
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

        [Fact, IsUnit]
        public async Task GetAllSubscribersAsync_should_invoke_BuildSelectAllSubscribers()
        {
            // Arrange

            // Act
            var result = await _repository.GetAllSubscribersAsync();

            // Assert
            _queryBuilderMock.Verify(x => x.BuildSelectAllSubscribers());
        }

        [Fact, IsUnit]
        public async Task GetAllSubscribersAsync_should_invoke_Query()
        {
            // Arrange

            // Act
            var result = await _repository.GetAllSubscribersAsync();

            // Assert
            _cosmosDbRepositoryMock.Verify(x => x.QueryAsync<SubscriberDocument>(It.IsAny<CosmosQuery>()));
        }

        [Fact, IsUnit]
        public async Task AddSubscriberInternalAsync_should_return_error_if_create_fails()
        {
            // Arrange
            var subscriberDocument = new SubscriberDocument
            {
                SubscriberName = "subscriberName",
                EventName = "eventName",
                WebhookSelectionRule = "rule",
                Endpoints = new EndpointSubdocument[] { }
            };
            var subscriber = new SubscriberEntity("subscriberName", "rule", new EventEntity("eventName"));
            _cosmosDbRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<SubscriberDocument>()))
                .ThrowsAsync(new CosmosException(string.Empty, System.Net.HttpStatusCode.Conflict, 0, string.Empty, 0));

            // Act
            var result = await _repository.AddSubscriberAsync(subscriber);

            // Assert
            result.Error.Should().BeOfType<CannotSaveEntityError>();
        }

    }
}
