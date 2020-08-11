using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Errors;
using CaptainHook.Domain.ValueObjects;
using CaptainHook.Storage.Cosmos.Models;
using CaptainHook.Storage.Cosmos.QueryBuilders;
using Eshopworld.Data.CosmosDb;
using Eshopworld.Data.CosmosDb.Exceptions;
using Eshopworld.Tests.Core;
using FluentAssertions;
using FluentAssertions.Common;
using FluentAssertions.Execution;
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
        public async Task GetSubscribersListAsync_WithInvalidEventName_ThrowsException(string eventName)
        {
            // Act
            Task act() => _repository.GetSubscribersListAsync(eventName);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(act);
        }        

        [Fact, IsUnit]
        public async Task GetSubscribersListAsync_WithValidEventName_InvokesBuildSelectForEventSubscribers()
        {
            // Arrange
            var eventName = "eventName";

            // Act
            var result = await _repository.GetSubscribersListAsync(eventName);

            // Assert
            _cosmosDbRepositoryMock.Verify(x => x.QueryAsync<SubscriberDocument>(It.IsAny<CosmosQuery>()));
        }

        [Fact, IsUnit]
        public async Task GetSubscribersListAsync_WithValidEventName_ReturnsCorrectModels()
        {
            // Arrange
            var eventName = "eventName";
            var endpoint = new EndpointSubdocument
            {
                HttpVerb = "POST",
                Uri = "http://test",
                Selector = "selector",
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
                Webhooks = new WebhookSubdocument
                {
                    SelectionRule = "rule",
                    Endpoints = new[] { endpoint }
                },
                Etag = "version1"
            };
            _cosmosDbRepositoryMock
                .Setup(x => x.QueryAsync<SubscriberDocument>(It.IsAny<CosmosQuery>()))
                .ReturnsAsync(new List<SubscriberDocument> { sampleDocument });

            var expectedSubscriberEntity = new SubscriberEntity(sampleDocument.SubscriberName, new EventEntity(eventName), "version1");
            var sampleAuthStoreEntity = new SecretStoreEntity(endpoint.Authentication.KeyVaultName, endpoint.Authentication.SecretName);
            var expectedAuthenticationEntity = new AuthenticationEntity(endpoint.Authentication.ClientId, sampleAuthStoreEntity, endpoint.Authentication.Uri, endpoint.Authentication.Type, endpoint.Authentication.Scopes);
            var expectedEndpointEntity = new EndpointEntity(endpoint.Uri, expectedAuthenticationEntity, endpoint.HttpVerb, endpoint.Selector);
            var expectedWebhooksEntity = new WebhooksEntity(sampleDocument.Webhooks.SelectionRule, new List<EndpointEntity> { expectedEndpointEntity });
            expectedSubscriberEntity.AddWebhooks(expectedWebhooksEntity);

            // Act
            var result = await _repository.GetSubscribersListAsync(eventName);

            // Assert
            result.Data.Should().BeEquivalentTo(new [] { expectedSubscriberEntity }, options => options.IgnoringCyclicReferences());
        }

        [Fact, IsUnit]
        public async Task GetSubscribersListAsync_WithValidEventName_MapsSubscriberDocumentToSubscriberEntityCorrectly()
        {
            // Arrange
            const string eventName = "eventName";
            var response = new[]
            {
                new SubscriberDocument
                {
                    SubscriberName = "subscriberName",
                    EventName = eventName,
                    Webhooks = new WebhookSubdocument
                    {
                        SelectionRule = "rule",
                        Endpoints = new[]
                        {
                            new EndpointSubdocument
                            {
                                HttpVerb = "POST",
                                Uri = "http://test",
                                Selector = "selector",
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
                    }
                }
            };

            _cosmosDbRepositoryMock
                .Setup(x => x.QueryAsync<SubscriberDocument>(It.IsAny<CosmosQuery>()))
                .ReturnsAsync(response);

            // Act
            var result = await _repository.GetSubscribersListAsync(eventName);

            // Assert
            var expected = new[]
            {
                new SubscriberEntity("subscriberName", new EventEntity(eventName))
                    .AddWebhooks(new WebhooksEntity("rule", new List<EndpointEntity>()
                    {
                        new EndpointEntity(
                            "http://test",
                            new AuthenticationEntity(
                                "clientid",
                                new SecretStoreEntity("keyvaultname", "secret"),
                                "uri",
                                "type",
                                new[] { "scope" }),
                            "POST",
                            "selector"),
                        new EndpointEntity("http://test2",
                            new AuthenticationEntity(
                                "clientid",
                                new SecretStoreEntity("keyvaultname", "secret"),
                                "uri",
                                "type",
                                new[] { "scope" }),
                            "GET",
                            "selector2")
                    }))
            };

            result.Data.Should().BeEquivalentTo(expected, options => options.IgnoringCyclicReferences());
        }

        [Fact, IsUnit]
        public async Task GetSubscriberAsync_WithInvalidSubscriberId_ThrowsException()
        {
            // Act
            Task act() => _repository.GetSubscriberAsync(null);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(act);
        }

        [Fact, IsUnit]
        public async Task GetSubscriberAsync_WithValidSubscriberId_InvokesBuildSelectSubscriber()
        {
            // Arrange
            var subscriberId = new SubscriberId("eventName", "subscriberName");

            // Act
            var result = await _repository.GetSubscriberAsync(subscriberId);

            // Assert
            _queryBuilderMock.Verify(x => x.BuildSelectSubscriber(subscriberId, subscriberId.EventName));
        }

        [Fact, IsUnit]
        public async Task GetSubscriberAsync_WithValidSubscriberId_InvokesQuery()
        {
            // Arrange
            var subscriberId = new SubscriberId("eventName", "subscriberName");

            // Act
            var result = await _repository.GetSubscriberAsync(subscriberId);

            // Assert
            _cosmosDbRepositoryMock.Verify(x => x.QueryAsync<SubscriberDocument>(It.IsAny<CosmosQuery>()));
        }

        [Fact, IsUnit]
        public async Task GetSubscriberAsync_WithValidSubscriberId_ReturnsCorrectModel()
        {
            // Arrange
            var eventName = "eventName";
            var endpoint = new EndpointSubdocument
            {
                HttpVerb = "POST",
                Uri = "http://test",
                Selector = "selector",
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
                Webhooks = new WebhookSubdocument
                {
                    SelectionRule = "rule",
                    Endpoints = new[] { endpoint }
                },
                Etag = "version1"
            };
            _cosmosDbRepositoryMock
                .Setup(x => x.QueryAsync<SubscriberDocument>(It.IsAny<CosmosQuery>()))
                .ReturnsAsync(new List<SubscriberDocument> { sampleDocument });

            var expectedSubscriberEntity = new SubscriberEntity(sampleDocument.SubscriberName, new EventEntity(eventName), "version1");
            var sampleAuthStoreEntity = new SecretStoreEntity(endpoint.Authentication.KeyVaultName, endpoint.Authentication.SecretName);
            var expectedAuthenticationEntity = new AuthenticationEntity(endpoint.Authentication.ClientId, sampleAuthStoreEntity, endpoint.Authentication.Uri, endpoint.Authentication.Type, endpoint.Authentication.Scopes);
            var expectedEndpointEntity = new EndpointEntity(endpoint.Uri, expectedAuthenticationEntity, endpoint.HttpVerb, endpoint.Selector);
            var expectedWebhooksEntity = new WebhooksEntity(sampleDocument.Webhooks.SelectionRule, new List<EndpointEntity> { expectedEndpointEntity });
            expectedSubscriberEntity.AddWebhooks(expectedWebhooksEntity);

            // Act
            var result = await _repository.GetSubscriberAsync(new SubscriberId("eventName", "subscriberName"));

            // Assert
            result.Data.Should().BeEquivalentTo(expectedSubscriberEntity, options => options.IgnoringCyclicReferences());
        }

        [Fact, IsUnit]
        public async Task GetSubscriberAsync_WithValidEventName_MapsSubscriberDocumentToSubscriberEntityCorrectly()
        {
            // Arrange
            const string eventName = "eventName";
            var response = new[]
            {
                new SubscriberDocument
                {
                    SubscriberName = "subscriberName",
                    EventName = eventName,
                    Webhooks = new WebhookSubdocument
                    {
                        SelectionRule = "rule",
                        Endpoints = new[]
                        {
                            new EndpointSubdocument
                            {
                                HttpVerb = "POST",
                                Uri = "http://test",
                                Selector = "selector",
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
                    }
                }
            };

            _cosmosDbRepositoryMock
                .Setup(x => x.QueryAsync<SubscriberDocument>(It.IsAny<CosmosQuery>()))
                .ReturnsAsync(response);

            // Act
            var result = await _repository.GetSubscriberAsync(new SubscriberId(eventName, "subscriberName"));

            // Assert
            var expected = new SubscriberEntity("subscriberName", new EventEntity(eventName))
                .AddWebhooks(new WebhooksEntity("rule", new List<EndpointEntity>()
                {
                    new EndpointEntity(
                        "http://test",
                        new AuthenticationEntity(
                            "clientid",
                            new SecretStoreEntity("keyvaultname", "secret"),
                            "uri",
                            "type",
                            new[] { "scope" }),
                        "POST",
                        "selector"),
                    new EndpointEntity("http://test2",
                        new AuthenticationEntity(
                            "clientid",
                            new SecretStoreEntity("keyvaultname", "secret"),
                            "uri",
                            "type",
                            new[] { "scope" }),
                        "GET",
                        "selector2")
                }));

            result.Data.Should().BeEquivalentTo(expected, options => options.IgnoringCyclicReferences());
        }

        [Fact, IsUnit]
        public async Task AddSubscriberAsync_WithInvalidSubscriberId_ThrowsException()
        {
            // Act
            Task act() => _repository.AddSubscriberAsync(null);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(act);
        }

        [Fact, IsUnit]
        public async Task AddSubscriberAsync_WithValidSubscriber_CallsCreateAsync()
        {
            // Arrange
            var subscriberDocument = new SubscriberDocument
            {
                SubscriberName = "subscriberName",
                EventName = "eventName",
                Webhooks = new WebhookSubdocument
                {
                    SelectionRule = "rule",
                    Endpoints = new EndpointSubdocument[] { }
                }
            };
            var subscriber = new SubscriberEntity("subscriberName", new EventEntity("eventName"));
            subscriber.AddWebhooks(new WebhooksEntity("rule"));
            _cosmosDbRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<SubscriberDocument>()))
                .ReturnsAsync(new DocumentContainer<SubscriberDocument>(subscriberDocument,"etag"));

            // Act
            var result = await _repository.AddSubscriberAsync(subscriber);

            // Assert
            _cosmosDbRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<SubscriberDocument>()));
        }

        [Fact, IsUnit]
        public async Task GetAllSubscribersAsync_WithNoParameters_CallsBuildSelectAllSubscribers()
        {
            // Arrange

            // Act
            var result = await _repository.GetAllSubscribersAsync();

            // Assert
            _queryBuilderMock.Verify(x => x.BuildSelectAllSubscribers());
        }

        [Fact, IsUnit]
        public async Task GetAllSubscribersAsync_WithNoParameters_CallsQueryMethod()
        {
            // Arrange

            // Act
            var result = await _repository.GetAllSubscribersAsync();

            // Assert
            _cosmosDbRepositoryMock.Verify(x => x.QueryAsync<SubscriberDocument>(It.IsAny<CosmosQuery>()));
        }

        [Fact, IsUnit]
        public async Task AddSubscriberAsync_WithErrorWhileSaving_ReturnsCannotSaveEntityError()
        {
            // Arrange
            var subscriberDocument = new SubscriberDocument
            {
                SubscriberName = "subscriberName",
                EventName = "eventName",
                Webhooks = new WebhookSubdocument
                {
                    SelectionRule = "rule",
                    Endpoints = new EndpointSubdocument[] { }
                }
            };
            var subscriber = new SubscriberEntity("subscriberName", new EventEntity("eventName"));
            _cosmosDbRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<SubscriberDocument>()))
                .ThrowsAsync(new CosmosException(string.Empty, System.Net.HttpStatusCode.Conflict, 0, string.Empty, 0));

            // Act
            var result = await _repository.AddSubscriberAsync(subscriber);

            // Assert
            result.Error.Should().BeOfType<CannotSaveEntityError>();
        }

        [Fact, IsUnit]
        public async Task UpdateSubscriberAsync_WithValidSubscriber_CallsRepoWithCorrectDocument()
        {
            // Arrange            
            var subscriber = new SubscriberEntity("subscriberName", new EventEntity("eventName"), "version1");
            var subscriberId = subscriber.Id.ToString();
            var subscriberDocument = new SubscriberDocument
            {
                Id = subscriberId,
                SubscriberName = "subscriberName",
                EventName = "eventName",
                Webhooks = new WebhookSubdocument
                {
                    SelectionRule = "rule",
                    Endpoints = new EndpointSubdocument[] { }
                },
                Etag = "version1"
            };

            subscriber.AddWebhooks(new WebhooksEntity("rule"));

            // Act
            await _repository.UpdateSubscriberAsync(subscriber);

            // Assert
            Func<SubscriberDocument, bool> validate = value =>
            {
                value.Should().BeEquivalentTo(subscriberDocument);
                return true;
            };

            _cosmosDbRepositoryMock.Verify(x => x.ReplaceAsync(
                subscriberId,
                It.Is<SubscriberDocument>(arg => validate(arg)),
                "version1"), Times.Once);
        }

        [Fact, IsUnit]
        public async Task UpdateSubscriberAsync_WithNullSubscriber_ThrowsException()
        {
            // Act
            Task act() => _repository.UpdateSubscriberAsync(null);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(act);
        }

        [Fact, IsUnit]
        public async Task UpdateSubscriberAsync_WithUnknownSubscriber_ThrowsCannotUpdateEntityError()
        {
            // Arrange
            var subscriberDocument = new SubscriberDocument
            {
                Id = "eventName-subscriberName",
                SubscriberName = "subscriberName",
                EventName = "eventName",
                Webhooks = new WebhookSubdocument
                {
                    SelectionRule = "rule",
                    Endpoints = new EndpointSubdocument[] { }
                }
            };
            var subscriber = new SubscriberEntity("subscriberName", new EventEntity("eventName"));
            subscriber.AddWebhooks(new WebhooksEntity("rule"));
            _cosmosDbRepositoryMock
                .Setup(x => x.ReplaceAsync(subscriberDocument.Id, subscriberDocument, null))
                .ThrowsAsync(new MissingDocumentException());

            // Act
            var result = await _repository.UpdateSubscriberAsync(subscriber);

            // Assert
            result.Error.Should().BeOfType<CannotUpdateEntityError>();
        }

        [Fact, IsUnit]
        public async Task UpdateSubscriberAsync_WithErrorWhileSaving_ReturnsCannotUpdateEntityError()
        {
            // Arrange
            var subscriberDocument = new SubscriberDocument
            {
                SubscriberName = "subscriberName",
                EventName = "eventName",
                Webhooks = new WebhookSubdocument
                {
                    SelectionRule = "rule",
                    Endpoints = new EndpointSubdocument[] { }
                }
            };
            var subscriber = new SubscriberEntity("subscriberName", new EventEntity("eventName"));
            _cosmosDbRepositoryMock
                .Setup(x => x.CreateAsync(subscriberDocument))
                .ThrowsAsync(new CosmosException(string.Empty, System.Net.HttpStatusCode.Conflict, 0, string.Empty, 0));

            // Act
            var result = await _repository.UpdateSubscriberAsync(subscriber);

            // Assert
            result.Error.Should().BeOfType<CannotUpdateEntityError>();
        }
    }
}
