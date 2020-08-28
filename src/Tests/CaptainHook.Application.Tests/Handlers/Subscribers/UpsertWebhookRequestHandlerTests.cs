using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Application.Handlers.Subscribers;
using CaptainHook.Application.Infrastructure.DirectorService;
using CaptainHook.Application.Infrastructure.Mappers;
using CaptainHook.Application.Requests.Subscribers;
using CaptainHook.Contract;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Errors;
using CaptainHook.Domain.Repositories;
using CaptainHook.Domain.Results;
using CaptainHook.Domain.ValueObjects;
using CaptainHook.TestsInfrastructure.Builders;
using Eshopworld.Tests.Core;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Xunit;

namespace CaptainHook.Application.Tests.Handlers.Subscribers
{
    public class UpsertWebhookRequestHandlerTests
    {
        private readonly Mock<ISubscriberRepository> _repositoryMock = new Mock<ISubscriberRepository>();
        private readonly Mock<IDirectorServiceProxy> _directorServiceMock = new Mock<IDirectorServiceProxy>();
        private readonly Mock<IDtoToEntityMapper> _dtoToEntityMapper = new Mock<IDtoToEntityMapper>(MockBehavior.Strict);

        private static readonly EndpointDto _defaultEndpointDto = new EndpointDtoBuilder().Create();
        private static readonly UpsertWebhookRequest _defaultUpsertRequest =
            new UpsertWebhookRequest("event", "subscriber", "*", _defaultEndpointDto);

        private static readonly SubscriberBuilder DefaultSubscriberBuilder = new SubscriberBuilder().WithEvent("event")
            .WithName("subscriber")
            .WithWebhookSelectionRule("$.Test")
            .WithWebhook(
                "https://blah.blah.eshopworld.com/oldwebhook/",
                "POST",
                "abc",
                authentication: new OidcAuthenticationEntity(
                    "captain-hook-id",
                    "kv-secret-name",
                    "https://blah-blah.sts.eshopworld.com",
                    new[] { "scope1" }));

        private UpsertWebhookRequestHandler Handler => new UpsertWebhookRequestHandler(
            _repositoryMock.Object,
            _directorServiceMock.Object,
            _dtoToEntityMapper.Object,
            new[]
            {
                TimeSpan.FromMilliseconds(10.0),
                TimeSpan.FromMilliseconds(10.0)
            });

        public UpsertWebhookRequestHandlerTests()
        {
            _dtoToEntityMapper.Setup(r => r.MapEndpoint(It.IsAny<EndpointDto>(), _defaultUpsertRequest.Selector))
                .Returns(new EndpointEntity(_defaultEndpointDto.Uri,
                    authentication: new OidcAuthenticationEntity(
                        clientId: ((OidcAuthenticationDto)_defaultEndpointDto.Authentication).ClientId,
                        clientSecretKeyName: ((OidcAuthenticationDto)_defaultEndpointDto.Authentication).ClientSecretKeyName,
                        uri: ((OidcAuthenticationDto)_defaultEndpointDto.Authentication).Uri,
                        scopes: ((OidcAuthenticationDto)_defaultEndpointDto.Authentication).Scopes.ToArray()
                    ), _defaultEndpointDto.HttpVerb, _defaultEndpointDto.Selector));
        }

        [Fact, IsUnit]
        public async Task When_SubscriberDoesNotExist_Then_NewOneWillBePassedToDirectorServiceAndStoredInRepository()
        {
            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync(new EntityNotFoundError("subscriber", "key"));
            _repositoryMock.Setup(r => r.AddSubscriberAsync(It.Is<SubscriberEntity>(entity => entity.Webhooks.Endpoints.Count() == 1)))
                .ReturnsAsync(new SubscriberEntity("subscriber"));
            _directorServiceMock.Setup(r => r.CreateReaderAsync(It.Is<SubscriberEntity>(rci => MatchReaderChangeInfo(rci, _defaultUpsertRequest))))
                .ReturnsAsync(true);
            
            var result = await Handler.Handle(_defaultUpsertRequest, CancellationToken.None);

            using var scope = new AssertionScope();
            result.IsError.Should().BeFalse();
            _repositoryMock.VerifyAll();
            _directorServiceMock.VerifyAll();
            _dtoToEntityMapper.VerifyAll();
        }

        [Fact, IsUnit]
        public async Task When_Subscriber_DoesExist_Then_SameEndpointReturned()
        {
            var subscriberEntity = DefaultSubscriberBuilder.Create();
            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync(subscriberEntity);
            _directorServiceMock.Setup(r => r.UpdateReaderAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(true);
            _repositoryMock.Setup(r => r.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(new SubscriberEntity("subscriber"));
            
            var result = await Handler.Handle(_defaultUpsertRequest, CancellationToken.None);

            using var scope = new AssertionScope();
            result.IsError.Should().BeFalse();
            result.Data.Should().BeEquivalentTo(_defaultUpsertRequest.Endpoint);
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.UpdateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Once);
            _dtoToEntityMapper.Verify(x => x.MapEndpoint(It.IsAny<EndpointDto>(), "*"), Times.Once);
            _repositoryMock.Verify(x => x.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Once);
        }

        [Fact, IsUnit]
        public async Task When_RepositoryFailsRetrievingSubscriberData_Then_OperationFails()
        {
            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync(new BusinessError("test error"));
            _repositoryMock.Setup(r => r.AddSubscriberAsync(It.Is<SubscriberEntity>(entity => entity.Webhooks.Endpoints.Count() == 1)))
                .ReturnsAsync(new SubscriberEntity("subscriber"));
            _directorServiceMock.Setup(r => r.CreateReaderAsync(It.Is<SubscriberEntity>(rci => MatchReaderChangeInfo(rci, _defaultUpsertRequest))))
                .ReturnsAsync(false);

            var result = await Handler.Handle(_defaultUpsertRequest, CancellationToken.None);

            using var scope = new AssertionScope();
            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<BusinessError>();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.CreateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Never);
            _repositoryMock.Verify(x => x.AddSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Never);
        }

        [Fact, IsUnit]
        public async Task When_DirectorServiceIsBusyReloading_Then_OperationFails()
        {
            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync(new EntityNotFoundError("subscriber", "key"));
            _repositoryMock.Setup(r => r.AddSubscriberAsync(It.Is<SubscriberEntity>(entity => entity.Webhooks.Endpoints.Count() == 1)))
                .ReturnsAsync(new SubscriberEntity("subscriber"));
            _directorServiceMock.Setup(r => r.CreateReaderAsync(It.Is<SubscriberEntity>(rci => MatchReaderChangeInfo(rci, _defaultUpsertRequest))))
                .ReturnsAsync(new DirectorServiceIsBusyError());
            
            var result = await Handler.Handle(_defaultUpsertRequest, CancellationToken.None);

            using var scope = new AssertionScope();
            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<DirectorServiceIsBusyError>();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.CreateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Once);
            _dtoToEntityMapper.Verify(x => x.MapEndpoint(It.IsAny<EndpointDto>(), "*"), Times.Once);
            _repositoryMock.Verify(x => x.AddSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Never);
        }

        [Fact, IsUnit]
        public async Task When_DirectorServiceFailsToRefreshReaderAsync_Then_OperationFails()
        {
            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync(new EntityNotFoundError("subscriber", "key"));
            _repositoryMock.Setup(r => r.AddSubscriberAsync(It.Is<SubscriberEntity>(entity => entity.Webhooks.Endpoints.Count() == 1)))
                .ReturnsAsync(new SubscriberEntity("subscriber"));
            _directorServiceMock.Setup(r => r.CreateReaderAsync(It.Is<SubscriberEntity>(rci => MatchReaderChangeInfo(rci, _defaultUpsertRequest))))
                .ReturnsAsync(new ReaderCreateError(new SubscriberEntity("subscriber")));
            
            var result = await Handler.Handle(_defaultUpsertRequest, CancellationToken.None);

            using var scope = new AssertionScope();
            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<ReaderCreateError>();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _dtoToEntityMapper.Verify(x => x.MapEndpoint(It.IsAny<EndpointDto>(), "*"), Times.Once);
            _directorServiceMock.Verify(x => x.CreateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Once);
            _repositoryMock.Verify(x => x.AddSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Never);
        }

        [Fact, IsUnit]
        public async Task When_RepositoryFailsSavingSubscriberAfterSuccessfulReaderCreation_Then_OperationFails()
        {
            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync(new EntityNotFoundError("subscriber", "key"));
            _repositoryMock.Setup(r => r.AddSubscriberAsync(It.Is<SubscriberEntity>(entity => entity.Webhooks.Endpoints.Count() == 1)))
                .ReturnsAsync(new BusinessError("test error"));
            _directorServiceMock.Setup(r => r.CreateReaderAsync(It.Is<SubscriberEntity>(rci => MatchReaderChangeInfo(rci, _defaultUpsertRequest))))
                .ReturnsAsync(true);
            
            var result = await Handler.Handle(_defaultUpsertRequest, CancellationToken.None);

            using var scope = new AssertionScope();
            result.IsError.Should().BeTrue();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.CreateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Once);
            _dtoToEntityMapper.Verify(x => x.MapEndpoint(It.IsAny<EndpointDto>(), "*"), Times.Once);
            _repositoryMock.Verify(x => x.AddSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Once);
        }

        [Fact, IsUnit]
        public async Task When_DocumentUpdateFails_Then_OperationIsRetried3Times()
        {
            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync(() => DefaultSubscriberBuilder.Create());
            _directorServiceMock.Setup(r => r.UpdateReaderAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(true);
            _repositoryMock.Setup(r => r.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(new CannotUpdateEntityError("dummy-type", new Exception()));
            
            var result = await Handler.Handle(_defaultUpsertRequest, CancellationToken.None);

            var expectedResult = new OperationResult<EndpointDto>(new CannotUpdateEntityError("dummy-type", new Exception()));
            result.Should().BeEquivalentTo(expectedResult);
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Exactly(3));
            _directorServiceMock.Verify(x => x.UpdateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Exactly(3));
            _dtoToEntityMapper.Verify(x => x.MapEndpoint(It.IsAny<EndpointDto>(), "*"), Times.Exactly(3));
            _repositoryMock.Verify(x => x.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Exactly(3));
        }

        [Fact, IsUnit]
        public async Task When_DocumentUpdateFails_Then_SucceedsOnSecondTry()
        {
            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync(() => DefaultSubscriberBuilder.Create());
            _directorServiceMock.Setup(r => r.UpdateReaderAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(true);
            _repositoryMock.SetupSequence(r => r.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(new CannotUpdateEntityError("dummy-type", new Exception()))
                .ReturnsAsync(() => DefaultSubscriberBuilder.Create());
            
            var result = await Handler.Handle(_defaultUpsertRequest, CancellationToken.None);

            var expectedResult = new OperationResult<EndpointDto>(_defaultUpsertRequest.Endpoint);
            result.Should().BeEquivalentTo(expectedResult);
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Exactly(2));
            _directorServiceMock.Verify(x => x.UpdateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Exactly(2));
            _dtoToEntityMapper.Verify(x => x.MapEndpoint(It.IsAny<EndpointDto>(), "*"), Times.Exactly(2));
            _repositoryMock.Verify(x => x.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Exactly(2));
        }

        [Fact, IsUnit]
        public async Task When_DocumentUpdateFailsAndDocumentIsRemoved_Then_TriesToAddOnSecondTry()
        {
            _repositoryMock.SetupSequence(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync(() => DefaultSubscriberBuilder.Create())
                .ReturnsAsync(new EntityNotFoundError("dummy-type", "dummy-key"));
            _directorServiceMock.Setup(r => r.UpdateReaderAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(true);
            _repositoryMock.SetupSequence(r => r.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(new CannotUpdateEntityError("dummy-type", new Exception()))
                .ReturnsAsync(() => DefaultSubscriberBuilder.Create());
            _directorServiceMock.Setup(x => x.CreateReaderAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(true);
            _repositoryMock.Setup(x => x.AddSubscriberAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(() => DefaultSubscriberBuilder.Create());
            
            var result = await Handler.Handle(_defaultUpsertRequest, CancellationToken.None);

            var expectedResult = new OperationResult<EndpointDto>(_defaultUpsertRequest.Endpoint);
            result.Should().BeEquivalentTo(expectedResult);
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Exactly(2));
            _directorServiceMock.Verify(x => x.UpdateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Once);
            _repositoryMock.Verify(x => x.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Once);
            _directorServiceMock.Verify(x => x.CreateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Once);
            _dtoToEntityMapper.Verify(x => x.MapEndpoint(It.IsAny<EndpointDto>(), "*"), Times.Exactly(2));
            _repositoryMock.Verify(x => x.AddSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Once);
        }

        private bool MatchReaderChangeInfo(in SubscriberEntity entity, UpsertWebhookRequest request)
        {
            var endpointEntity = entity.Webhooks.Endpoints.First();

            return entity.Name == request.SubscriberName
                   && entity.ParentEvent.Name == request.EventName
                   && endpointEntity.Uri == request.Endpoint.Uri
                   && MatchAuthentications(endpointEntity.Authentication, request.Endpoint.Authentication);
        }

        private static bool MatchAuthentications(AuthenticationEntity entity, AuthenticationDto dto)
        {
            return entity switch
            {
                BasicAuthenticationEntity basicEntity => MatchBasicAuthentication(basicEntity, (BasicAuthenticationDto)dto),
                OidcAuthenticationEntity oidcEntity => MatchOidcAuthentication(oidcEntity, (OidcAuthenticationDto)dto),
                _ => false
            };
        }

        private static bool MatchOidcAuthentication(OidcAuthenticationEntity entity, OidcAuthenticationDto dto)
        {
            return
                entity.ClientId == dto.ClientId &&
                entity.Uri == dto.Uri &&
                entity.Scopes.SequenceEqual(dto.Scopes);
        }

        private static bool MatchBasicAuthentication(BasicAuthenticationEntity entity, BasicAuthenticationDto dto)
        {
            return
                entity.Username == dto.Username &&
                entity.Password == dto.Password;
        }

        private static bool MatchUriTransforms(UriTransformDto dto, UriTransformEntity entity)
        {
            if (dto == null && entity == null)
                return true;

            if (dto.Replace.Count != entity.Replace.Count)
                return false;

            return dto.Replace.SequenceEqual(entity.Replace);
        }
    }
}
