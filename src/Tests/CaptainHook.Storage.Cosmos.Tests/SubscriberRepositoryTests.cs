using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Errors;
using CaptainHook.Domain.ValueObjects;
using CaptainHook.Storage.Cosmos.Models;
using CaptainHook.Storage.Cosmos.QueryBuilders;
using CaptainHook.TestsInfrastructure.Builders;
using Eshopworld.Data.CosmosDb;
using Eshopworld.Data.CosmosDb.Exceptions;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Moq;
using Newtonsoft.Json;
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
                new object[] { "POST", null, null },
                new object[] { "PUT", OidcAuthenticationEntity, OidcAuthenticationSubdocument },
                new object[] { "PUT", BasicAuthenticationEntity, BasicAuthenticationSubdocument },
                new object[] { "PUT", null, null },
                new object[] { "GET", OidcAuthenticationEntity, OidcAuthenticationSubdocument },
                new object[] { "GET", BasicAuthenticationEntity, BasicAuthenticationSubdocument },
                new object[] { "GET", null, null }
            };

        private SubscriberEntity CreateSubscriberEntity(string httpVerb = "POST", AuthenticationEntity authenticationEntity = null, string etag = null)
        {
            var replacements = new Dictionary<string, string> { { "ordercode", "$.OrderCode" } };

            var subscriberEntity = new SubscriberBuilder()
                .WithName("a-test-subscriber-name")
                .WithEvent("a-test-event-name")
                .WithWebhooksSelectionRule("$.TestSelector")
                .WithWebhooksUriTransform(new UriTransformEntity(replacements))
                .WithWebhook("http://www.test.uri/path", httpVerb, "Selector1", authenticationEntity ?? OidcAuthenticationEntity)
                .WithCallbacksSelectionRule("$.TestSelector")
                .WithCallbacksUriTransform(new UriTransformEntity(replacements))
                .WithCallback("http://www.test.uri/callback/path", httpVerb, "Selector1", authenticationEntity ?? OidcAuthenticationEntity)
                .WithDlqhooksSelectionRule("$.DlqSelector")
                .WithDlqhooksUriTransform(new UriTransformEntity(replacements))
                .WithDlqhook("http://www.test.uri/dlq/path", httpVerb, "DlqSelector", authenticationEntity ?? OidcAuthenticationEntity)
                .WithEtag(etag)
                .Create();

            return subscriberEntity;
        }

        private SubscriberDocument CreateSubscriberDocument(string httpVerb = "POST", object authenticationSubdocument = null, string etag = null)
        {
            var replacements = new Dictionary<string, string> { { "ordercode", "$.OrderCode" } };

            var endpointWebhookSubdocument = new EndpointSubdocumentBuilder()
                .With(x => x.HttpVerb, httpVerb)
                .With(x => x.Selector, "Selector1")
                .With(x => x.Authentication, (AuthenticationSubdocument)authenticationSubdocument ?? OidcAuthenticationSubdocument)
                .With(x => x.Uri, "http://www.test.uri/path")
                .Create();

            var endpointCallbackSubdocument = new EndpointSubdocumentBuilder()
                .With(x => x.HttpVerb, httpVerb)
                .With(x => x.Selector, "Selector1")
                .With(x => x.Authentication, (AuthenticationSubdocument)authenticationSubdocument ?? OidcAuthenticationSubdocument)
                .With(x => x.Uri, "http://www.test.uri/callback/path")
                .Create();

            var endpointDlqhooksSubdocument = new EndpointSubdocumentBuilder()
                .With(x => x.HttpVerb, httpVerb)
                .With(x => x.Selector, "DlqSelector")
                .With(x => x.Authentication, (AuthenticationSubdocument)authenticationSubdocument ?? OidcAuthenticationSubdocument)
                .With(x => x.Uri, "http://www.test.uri/dlq/path")
                .Create();

            var webhooksSubdocument = new WebhooksSubdocumentBuilder()
                .With(x => x.SelectionRule, "$.TestSelector")
                .With(x => x.UriTransform, new UriTransformSubdocument(replacements))
                .With(x => x.Endpoints, new EndpointSubdocument[] { endpointWebhookSubdocument })
                .With(x => x.PayloadTransform, "$")
                .Create();

            var callbacksSubdocument = new WebhooksSubdocumentBuilder()
                .With(x => x.SelectionRule, "$.TestSelector")
                .With(x => x.UriTransform, new UriTransformSubdocument(replacements))
                .With(x => x.Endpoints, new EndpointSubdocument[] { endpointCallbackSubdocument })
                .Create();

            var dlqhooksSubdocument = new WebhooksSubdocumentBuilder()
                .With(x => x.SelectionRule, "$.DlqSelector")
                .With(x => x.UriTransform, new UriTransformSubdocument(replacements))
                .With(x => x.Endpoints, new EndpointSubdocument[] {endpointDlqhooksSubdocument})
                .With(x => x.PayloadTransform, "$")
                .Create();

            var subscriberDocument = new SubscriberDocumentBuilder()
                .With(x => x.Webhooks, webhooksSubdocument)
                .With(x => x.Callbacks, callbacksSubdocument)
                .With(x => x.DlqHooks, dlqhooksSubdocument)
                .With(x => x.Etag, etag)
                .Create();

            return subscriberDocument;
        }

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

        [Theory, IsUnit]
        [MemberData(nameof(Data))]
        public async Task GetSubscribersListAsync_WithValidEventName_ReturnsCorrectModels(string httpVerb, AuthenticationEntity authenticationEntity, object authenticationSubdocument)
        {
            var subscriberDocument = CreateSubscriberDocument(httpVerb, authenticationSubdocument);

            var subscriberEntity = CreateSubscriberEntity(httpVerb, authenticationEntity);

            _cosmosDbRepositoryMock
                .Setup(x => x.QueryAsync<dynamic>(It.IsAny<CosmosQuery>()))
                .ReturnsAsync(new List<dynamic> { ConvertToDynamic(subscriberDocument) });

            // Act
            var result = await _repository.GetSubscribersListAsync(subscriberDocument.EventName);

            // Assert
            result.Data.Should().BeEquivalentTo(new[] { subscriberEntity }, options => options.IgnoringCyclicReferences());
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

        [Theory, IsUnit]
        [MemberData(nameof(Data))]
        public async Task GetSubscriberAsync_WithValidSubscriberId_ReturnsCorrectModel(string httpVerb, AuthenticationEntity authenticationEntity, object authenticationSubdocument)
        {
            // Arrange
            var subscriberDocument = CreateSubscriberDocument(httpVerb, authenticationSubdocument);

            var subscriberEntity = CreateSubscriberEntity(httpVerb, authenticationEntity);

            _cosmosDbRepositoryMock
                .Setup(x => x.QueryAsync<dynamic>(It.IsAny<CosmosQuery>()))
                .ReturnsAsync(new List<dynamic> { ConvertToDynamic(subscriberDocument) });

            // Act
            var subscriberId = new SubscriberId(subscriberDocument.EventName, subscriberDocument.SubscriberName);
            var result = await _repository.GetSubscriberAsync(subscriberId);

            // Assert
            result.Data.Should().BeEquivalentTo(subscriberEntity, options => options.IgnoringCyclicReferences());
        }

        [Fact, IsUnit]
        public async Task AddSubscriberAsync_WithInvalidSubscriberId_ThrowsException()
        {
            // Act
            Task act() => _repository.AddSubscriberAsync(null);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(act);
        }

        [Theory, IsUnit]
        [MemberData(nameof(Data))]
        public async Task AddSubscriberAsync_WithValidSubscriber_CallsCreateAsync(string httpVerb, AuthenticationEntity authenticationEntity, object authenticationSubdocument)
        {
            // Arrange
            var subscriberDocument = CreateSubscriberDocument(httpVerb, authenticationSubdocument);

            var subscriberEntity = CreateSubscriberEntity(httpVerb, authenticationEntity);

            // Act
            await _repository.AddSubscriberAsync(subscriberEntity);

            // Assert
            Func<object, bool> validate = value =>
            {
                value.Should().BeEquivalentTo(subscriberDocument);
                return true;
            };

            _cosmosDbRepositoryMock.Verify(x => x.CreateAsync(It.Is<object>(arg => validate(arg))), Times.Once);
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

        [Theory, IsUnit]
        [MemberData(nameof(Data))]
        public async Task AddSubscriberAsync_WithErrorWhileSaving_ReturnsCannotSaveEntityError(string httpVerb, AuthenticationEntity authenticationEntity, object authenticationSubdocument)
        {
            // Arrange
            var subscriberDocument = CreateSubscriberDocument(httpVerb, authenticationSubdocument);

            var subscriberEntity = CreateSubscriberEntity(httpVerb, authenticationEntity);

            _cosmosDbRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<SubscriberDocument>()))
                .ThrowsAsync(new CosmosException(string.Empty, System.Net.HttpStatusCode.Conflict, 0, string.Empty, 0));

            // Act
            var result = await _repository.AddSubscriberAsync(subscriberEntity);

            // Assert
            result.Error.Should().BeOfType<CannotSaveEntityError>();
        }

        [Theory, IsUnit]
        [MemberData(nameof(Data))]
        public async Task AddSubscriberAsync_WithFullSubscriber_CallsRepoWithCorrectDocument(string httpVerb, AuthenticationEntity authenticationEntity, object authenticationSubdocument)
        {
            // Arrange            
            var subscriberDocument = CreateSubscriberDocument(httpVerb, authenticationSubdocument);

            var subscriberEntity = CreateSubscriberEntity(httpVerb, authenticationEntity);

            // Act
            await _repository.AddSubscriberAsync(subscriberEntity);

            // Assert
            Func<object, bool> validate = value =>
            {
                value.Should().BeEquivalentTo(subscriberDocument);
                return true;
            };

            _cosmosDbRepositoryMock.Verify(x => x.CreateAsync(It.Is<object>(arg => validate(arg))), Times.Once);
        }

        [Theory, IsUnit]
        [MemberData(nameof(Data))]
        public async Task UpdateSubscriberAsync_WithValidSubscriber_CallsRepoWithCorrectDocument(string httpVerb, AuthenticationEntity authenticationEntity, object authenticationSubdocument)
        {
            // Arrange            
            var subscriberDocument = CreateSubscriberDocument(httpVerb, authenticationSubdocument, "version1");

            var subscriberEntity = CreateSubscriberEntity(httpVerb, authenticationEntity, "version1");

            // Act
            await _repository.UpdateSubscriberAsync(subscriberEntity);

            // Assert
            Func<object, bool> validate = value =>
            {
                value.Should().BeEquivalentTo(subscriberDocument);
                return true;
            };

            _cosmosDbRepositoryMock.Verify(x => x.ReplaceAsync(
                subscriberEntity.Id,
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
            var subscriberDocument = CreateSubscriberDocument();

            var subscriberEntity = CreateSubscriberEntity();

            _cosmosDbRepositoryMock
                .Setup(x => x.ReplaceAsync(subscriberDocument.Id, subscriberDocument, null))
                .ThrowsAsync(new MissingDocumentException());

            // Act
            var result = await _repository.UpdateSubscriberAsync(subscriberEntity);

            // Assert
            result.Error.Should().BeOfType<CannotUpdateEntityError>();
        }

        [Fact, IsUnit]
        public async Task UpdateSubscriberAsync_WithErrorWhileSaving_ReturnsCannotUpdateEntityError()
        {
            // Arrange
            var subscriberDocument = CreateSubscriberDocument();

            var subscriberEntity = CreateSubscriberEntity();

            _cosmosDbRepositoryMock
                .Setup(x => x.CreateAsync(subscriberDocument))
                .ThrowsAsync(new CosmosException(string.Empty, System.Net.HttpStatusCode.Conflict, 0, string.Empty, 0));

            // Act
            var result = await _repository.UpdateSubscriberAsync(subscriberEntity);

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
            var subscriberDocument = CreateSubscriberDocument();

            var subscriberEntity = CreateSubscriberEntity();

            _cosmosDbRepositoryMock
                .Setup(x => x.DeleteAsync<SubscriberDocument>(subscriberDocument.Id, subscriberDocument.Pk))
                .ReturnsAsync(false);

            // Act
            var result = await _repository.RemoveSubscriberAsync(subscriberEntity.Id);

            // Assert
            result.Error.Should().BeOfType<CannotDeleteEntityError>();
        }

        [Fact, IsUnit]
        public async Task RemoveSubscriberAsync_WithValidSubscriber_ReturnsNoError()
        {
            // Arrange
            var subscriberDocument = CreateSubscriberDocument();

            var subscriberEntity = CreateSubscriberEntity();

            _cosmosDbRepositoryMock
                .Setup(x => x.DeleteAsync<SubscriberDocument>(subscriberDocument.Id, subscriberDocument.Pk))
                .ReturnsAsync(true);

            // Act
            var result = await _repository.RemoveSubscriberAsync(subscriberEntity.Id);

            // Assert
            result.IsError.Should().BeFalse();
        }

        private static dynamic ConvertToDynamic(SubscriberDocument document)
        {
            return JsonConvert.SerializeObject(document);
        }
    }
}
