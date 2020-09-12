using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Errors;
using CaptainHook.Domain.ValueObjects;
using CaptainHook.Storage.Cosmos.Models;
using CaptainHook.Storage.Cosmos.QueryBuilders;
using Eshopworld.Data.CosmosDb;
using Eshopworld.Data.CosmosDb.Exceptions;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Moq;
using Newtonsoft.Json;
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

        private readonly SubscriberRepository _repository;

        private static readonly OidcAuthenticationSubdocument OidcAuthenticationSubdocument = new OidcAuthenticationSubdocument
        {
            Scopes = new string[] { "scope1", "scope2" },
            SecretName = "secret-key-name",
            ClientId = "client_id",
            Uri = "http://www.sts.uri.com/token"
        };

        private static readonly BasicAuthenticationSubdocument BasicAuthenticationSubdocument = new BasicAuthenticationSubdocument
        {
            Username = "username",
            PasswordKeyName = "password-key-name"
        };

        private static readonly OidcAuthenticationEntity OidcAuthenticationEntity =
            new OidcAuthenticationEntity("client_id", "secret-key-name", "http://www.sts.uri.com/token", new string[] { "scope1", "scope2" });

        private static readonly BasicAuthenticationEntity BasicAuthenticationEntity =
            new BasicAuthenticationEntity("username", "password-key-name");

        public static IEnumerable<object[]> Data =>
            new List<object[]>
            {
                new object[] { "POST", OidcAuthenticationEntity, OidcAuthenticationSubdocument },
                new object[] { "POST", BasicAuthenticationEntity, BasicAuthenticationSubdocument },
                new object[] { "PUT", OidcAuthenticationEntity, OidcAuthenticationSubdocument },
                new object[] { "PUT", BasicAuthenticationEntity, BasicAuthenticationSubdocument },
                new object[] { "GET", OidcAuthenticationEntity, OidcAuthenticationSubdocument },
                new object[] { "GET", BasicAuthenticationEntity, BasicAuthenticationSubdocument }
            };

        public SubscriberRepositoryTests()
        {
            _cosmosDbRepositoryMock = new Mock<ICosmosDbRepository>();
            _cosmosDbRepositoryMock
                .Setup(x => x.UseCollection(It.IsAny<string>(), It.IsAny<string>()));

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
            _cosmosDbRepositoryMock.Verify(x => x.QueryAsync<dynamic>(It.IsAny<CosmosQuery>()));
        }

        [Fact, IsUnit]
        public async Task GetSubscribersListAsync_WithValidEventName_ReturnsCorrectModels()
        {
            // Arrange
            var eventName = "eventName";
            var auth = new OidcAuthenticationSubdocument
            {
                ClientId = "clientid",
                Scopes = new string[] { "scope" },
                SecretName = "secret",
                Uri = "uri"
            };
            var endpoint = new EndpointSubdocument
            {
                HttpVerb = "POST",
                Uri = "http://test",
                Selector = "selector",
                Authentication = auth
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
                .Setup(x => x.QueryAsync<dynamic>(It.IsAny<CosmosQuery>()))
                .ReturnsAsync(new List<dynamic> { ConvertToDynamic(sampleDocument) });

            var expectedSubscriberEntity = new SubscriberEntity(sampleDocument.SubscriberName, new EventEntity(eventName), "version1");
            var expectedAuthenticationEntity = new OidcAuthenticationEntity(auth.ClientId, auth.SecretName, auth.Uri, auth.Scopes);
            var expectedEndpointEntity = new EndpointEntity(endpoint.Uri, expectedAuthenticationEntity, endpoint.HttpVerb, endpoint.Selector);
            var expectedWebhooksEntity = new WebhooksEntity(sampleDocument.Webhooks.SelectionRule, new List<EndpointEntity> { expectedEndpointEntity });
            expectedSubscriberEntity.AddWebhooks(expectedWebhooksEntity);

            // Act
            var result = await _repository.GetSubscribersListAsync(eventName);

            // Assert
            result.Data.Should().BeEquivalentTo(new[] { expectedSubscriberEntity }, options => options.IgnoringCyclicReferences());
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
                                Authentication = OidcAuthenticationSubdocument
                            },
                            new EndpointSubdocument
                            {
                                HttpVerb = "GET",
                                Uri = "http://test2",
                                Selector = "selector2",
                                Authentication = OidcAuthenticationSubdocument
                            }
                        }
                    }
                }
            };

            _cosmosDbRepositoryMock
                .Setup(x => x.QueryAsync<dynamic>(It.IsAny<CosmosQuery>()))
                .ReturnsAsync(response.Select(ConvertToDynamic));

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
                            OidcAuthenticationEntity,
                            "POST",
                            "selector"),
                        new EndpointEntity("http://test2",
                            OidcAuthenticationEntity,
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
            _cosmosDbRepositoryMock.Verify(x => x.QueryAsync<dynamic>(It.IsAny<CosmosQuery>()));
        }

        [Fact, IsUnit]
        public async Task GetSubscriberAsync_WithValidSubscriberId_ReturnsCorrectModel()
        {
            // Arrange
            var eventName = "eventName";
            var auth = new OidcAuthenticationSubdocument
            {
                ClientId = "clientid",
                Scopes = new string[] { "scope" },
                SecretName = "secret",
                Uri = "uri"
            };
            var endpoint = new EndpointSubdocument
            {
                HttpVerb = "POST",
                Uri = "http://test",
                Selector = "selector",
                Authentication = auth
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
                .Setup(x => x.QueryAsync<dynamic>(It.IsAny<CosmosQuery>()))
                .ReturnsAsync(new List<dynamic> { ConvertToDynamic(sampleDocument) });

            var expectedSubscriberEntity = new SubscriberEntity(sampleDocument.SubscriberName, new EventEntity(eventName), "version1");
            var expectedAuthenticationEntity = new OidcAuthenticationEntity(auth.ClientId, auth.SecretName, auth.Uri, auth.Scopes);
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
                                Authentication = new OidcAuthenticationSubdocument
                                {
                                    ClientId = "clientid",
                                    Scopes = new[] { "scope" },
                                    SecretName = "secret",
                                    Uri = "uri"
                                }
                            },
                            new EndpointSubdocument
                            {
                                HttpVerb = "GET",
                                Uri = "http://test2",
                                Selector = "selector2",
                                Authentication = new OidcAuthenticationSubdocument
                                {
                                    ClientId = "clientid",
                                    Scopes = new[] { "scope" },
                                    SecretName = "secret",
                                    Uri = "uri"
                                }
                            }
                        }
                    }
                }
            };

            _cosmosDbRepositoryMock
                .Setup(x => x.QueryAsync<dynamic>(It.IsAny<CosmosQuery>()))
                .ReturnsAsync(response.Select(ConvertToDynamic));

            // Act
            var result = await _repository.GetSubscriberAsync(new SubscriberId(eventName, "subscriberName"));

            // Assert
            var expected = new SubscriberEntity("subscriberName", new EventEntity(eventName))
                .AddWebhooks(new WebhooksEntity("rule", new List<EndpointEntity>()
                {
                    new EndpointEntity(
                        "http://test",
                        new OidcAuthenticationEntity(
                            "clientid",
                            "secret",
                            "uri",
                            new[] { "scope" }),
                        "POST",
                        "selector"),
                    new EndpointEntity("http://test2",
                        new OidcAuthenticationEntity(
                            "clientid",
                            "secret",
                            "uri",
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
                .Setup(x => x.CreateAsync<dynamic>(It.IsAny<SubscriberDocument>()))
                .ReturnsAsync(new DocumentContainer<dynamic>(subscriberDocument, "etag"));

            // Act
            var result = await _repository.AddSubscriberAsync(subscriber);

            // Assert
            _cosmosDbRepositoryMock.Verify(x => x.CreateAsync<dynamic>(It.IsAny<SubscriberDocument>()));
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
            _cosmosDbRepositoryMock.Verify(x => x.QueryAsync<dynamic>(It.IsAny<CosmosQuery>()));
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

        [Theory, IsUnit]
        [MemberData(nameof(Data))]
        public async Task AddSubscriberAsync_WithFullSubscriber_CallsRepoWithCorrectDocument(string verb, AuthenticationEntity authenticationEntity, object authenticationSubdocument)
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
                    Endpoints = new EndpointSubdocument[]
                    {
                        new EndpointSubdocument
                        {
                            Selector = "selector1",
                            HttpVerb = verb,
                            Authentication = (AuthenticationSubdocument)authenticationSubdocument,
                            Uri = "htttp://www.uri1.com"
                        },
                        new EndpointSubdocument
                        {
                            Selector = "selector2",
                            HttpVerb = verb,
                            Authentication = (AuthenticationSubdocument)authenticationSubdocument,
                            Uri = "htttp://www.uri2.com"
                        }
                    }
                },
                Callbacks = new WebhookSubdocument
                {
                    SelectionRule = "rule",
                    Endpoints = new EndpointSubdocument[]
                    {
                        new EndpointSubdocument
                        {
                            Selector = "selector1",
                            HttpVerb = verb,
                            Authentication = (AuthenticationSubdocument)authenticationSubdocument,
                            Uri = "htttp://www.uri1.com"
                        },
                        new EndpointSubdocument
                        {
                            Selector = "selector2",
                            HttpVerb = verb,
                            Authentication = (AuthenticationSubdocument)authenticationSubdocument,
                            Uri = "htttp://www.uri2.com"
                        }
                    }
                }
            };

            subscriber.AddWebhooks(new WebhooksEntity("rule", new List<EndpointEntity>()
            {
                new EndpointEntity("htttp://www.uri1.com", authenticationEntity, verb, "selector1"),
                new EndpointEntity("htttp://www.uri2.com", authenticationEntity, verb, "selector2")
            }));

            subscriber.AddCallbacks(new WebhooksEntity("rule", new List<EndpointEntity>()
            {
                new EndpointEntity("htttp://www.uri1.com", authenticationEntity, verb, "selector1"),
                new EndpointEntity("htttp://www.uri2.com", authenticationEntity, verb, "selector2")
            }));

            // Act
            await _repository.AddSubscriberAsync(subscriber);

            // Assert
            Func<object, bool> validate = value =>
            {
                value.Should().BeEquivalentTo(subscriberDocument);
                return true;
            };

            _cosmosDbRepositoryMock.Verify(x => x.CreateAsync(It.Is<object>(arg => validate(arg))), Times.Once);
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
                }
            };

            subscriber.AddWebhooks(new WebhooksEntity("rule", new List<EndpointEntity>()));

            // Act
            await _repository.UpdateSubscriberAsync(subscriber);

            // Assert
            Func<object, bool> validate = value =>
            {
                value.Should().BeEquivalentTo(subscriberDocument);
                return true;
            };

            _cosmosDbRepositoryMock.Verify(x => x.ReplaceAsync(
                subscriberId,
                It.Is<object>(arg => validate(arg)),
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

        [Fact, IsUnit]
        public async Task RemoveSubscriberAsync_WithNullSubscriberId_ThrowsException()
        {
            // Act
            Task act() => _repository.RemoveSubscriberAsync(null);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(act);
        }

        [Fact, IsUnit]
        public async Task RemoveSubscriberAsync_WithUnknownSubscriber_ReturnsCannotDeleteEntityError()
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
            var subscriberId = new SubscriberId("eventName", "subscriberName");
            _cosmosDbRepositoryMock
                .Setup(x => x.DeleteAsync<SubscriberDocument>(subscriberDocument.Id, subscriberDocument.Pk))
                .ReturnsAsync(false);

            // Act
            var result = await _repository.RemoveSubscriberAsync(subscriberId);

            // Assert
            result.Error.Should().BeOfType<CannotDeleteEntityError>();
        }

        [Fact, IsUnit]
        public async Task RemoveSubscriberAsync_WithValidSubscriber_ReturnsNoError()
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
            var subscriberId = new SubscriberId("eventName", "subscriberName");
            _cosmosDbRepositoryMock
                .Setup(x => x.DeleteAsync<SubscriberDocument>(subscriberDocument.Id, subscriberDocument.Pk))
                .ReturnsAsync(true);

            // Act
            var result = await _repository.RemoveSubscriberAsync(subscriberId);

            // Assert
            result.IsError.Should().BeFalse();
        }

        private static dynamic ConvertToDynamic(SubscriberDocument document)
        {
            return JsonConvert.SerializeObject(document);
        }
    }
}
