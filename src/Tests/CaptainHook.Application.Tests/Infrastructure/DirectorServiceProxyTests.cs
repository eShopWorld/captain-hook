using System.Collections.Generic;
using System.Threading.Tasks;
using CaptainHook.Application.Infrastructure.DirectorService;
using CaptainHook.Application.Infrastructure.DirectorService.Remoting;
using CaptainHook.Application.Infrastructure.Mappers;
using CaptainHook.Common.Configuration;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Errors;
using CaptainHook.TestsInfrastructure.Builders;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Moq;
using Xunit;

namespace CaptainHook.Application.Tests.Infrastructure
{
    public class DirectorServiceProxyTests
    {
        private readonly Mock<IDirectorServiceRemoting> _directorServiceMock = new Mock<IDirectorServiceRemoting>();
        private readonly Mock<ISubscriberEntityToConfigurationMapper> _mapperMock = new Mock<ISubscriberEntityToConfigurationMapper>();

        DirectorServiceProxy Proxy =>new DirectorServiceProxy(_mapperMock.Object, _directorServiceMock.Object);

        public DirectorServiceProxyTests()
        {
            _mapperMock.Setup(x => x.MapSubscriber(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(new List<SubscriberConfiguration> { new SubscriberConfiguration() });
        }

        [Fact, IsUnit]
        public async Task When_CreationSucceed_Then_TrueIsReturned()
        {
            _directorServiceMock.Setup(x => x.ProvisionReaderAsync(It.IsAny<SubscriberConfiguration>()))
                .ReturnsAsync(ReaderProvisionResult.Created);

            var result = await Proxy.ProvisionReaderAsync(new SubscriberBuilder().Create());

            result.IsError.Should().BeFalse();
            result.Data.Should().BeTrue();
        }

        [Fact, IsUnit]
        public async Task When_UpdateSucceed_Then_TrueIsReturned()
        {
            _directorServiceMock.Setup(x => x.ProvisionReaderAsync(It.IsAny<SubscriberConfiguration>()))
                .ReturnsAsync(ReaderProvisionResult.Updated);

            var result = await Proxy.ProvisionReaderAsync(new SubscriberBuilder().Create());

            result.IsError.Should().BeFalse();
            result.Data.Should().BeTrue();
        }

        [Fact, IsUnit]
        public async Task When_ReaderAlreadyExist_Then_TrueIsReturned()
        {
            _directorServiceMock.Setup(x => x.ProvisionReaderAsync(It.IsAny<SubscriberConfiguration>()))
                .ReturnsAsync(ReaderProvisionResult.ReaderAlreadyExists);

            var result = await Proxy.ProvisionReaderAsync(new SubscriberBuilder().Create());

            result.IsError.Should().BeFalse();
            result.Data.Should().BeTrue();
        }

        [Fact, IsUnit]
        public async Task When_DirectorServiceIsBusy_Then_DirectorServiceIsBusyErrorReturned()
        {
            _directorServiceMock.Setup(x => x.ProvisionReaderAsync(It.IsAny<SubscriberConfiguration>()))
                .ReturnsAsync(ReaderProvisionResult.DirectorIsBusy);

            var result = await Proxy.ProvisionReaderAsync(new SubscriberBuilder().Create());

            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<DirectorServiceIsBusyError>();
        }

        [Fact, IsUnit]
        public async Task When_ReaderCreationFailed_Then_ReaderCreationErrorReturned()
        {
            _directorServiceMock.Setup(x => x.ProvisionReaderAsync(It.IsAny<SubscriberConfiguration>()))
                .ReturnsAsync(ReaderProvisionResult.CreateFailed | ReaderProvisionResult.ReaderAlreadyExists);

            var result = await Proxy.ProvisionReaderAsync(new SubscriberBuilder().Create());

            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<ReaderCreationError>();
        }

        [Fact, IsUnit]
        public async Task When_ReaderDeletionFailed_Then_ReaderDeletionErrorReturned()
        {
            _directorServiceMock.Setup(x => x.ProvisionReaderAsync(It.IsAny<SubscriberConfiguration>()))
                .ReturnsAsync(ReaderProvisionResult.CreateFailed | ReaderProvisionResult.ReaderAlreadyExists | ReaderProvisionResult.Created);

            var result = await Proxy.ProvisionReaderAsync(new SubscriberBuilder().Create());

            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<ReaderDeletionError>();
        }

        [Fact, IsUnit]
        public async Task When_DirectorServiceResultNotSet_Then_GenericErrorReturned()
        {
            _directorServiceMock.Setup(x => x.ProvisionReaderAsync(It.IsAny<SubscriberConfiguration>()))
                .ReturnsAsync(ReaderProvisionResult.None);

            var result = await Proxy.ProvisionReaderAsync(new SubscriberBuilder().Create());

            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<BusinessError>();
        }
    }
}
