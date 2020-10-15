using System.Collections.Generic;
using System.Threading.Tasks;
using CaptainHook.Application.Infrastructure.DirectorService;
using CaptainHook.Application.Infrastructure.DirectorService.Remoting;
using CaptainHook.Common;
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

        DirectorServiceProxy Proxy => new DirectorServiceProxy(_directorServiceMock.Object);

        [Fact, IsUnit]
        public async Task When_Success_Then_SubscriberConfigurationIsReturned()
        {
            _directorServiceMock.Setup(x => x.ApplyReaderChange(It.IsAny<ReaderChangeBase>()))
                .ReturnsAsync(ReaderChangeResult.Success);

            var result = await Proxy.CallDirectorServiceAsync(new UpdateReader(new SubscriberConfigurationBuilder().Create()));

            result.IsError.Should().BeFalse();
            result.Data.Should().NotBeNull();
        }

        [Fact, IsUnit]
        public async Task When_ReaderDoesNotExist_Then_ErrorIsReturned()
        {
            _directorServiceMock.Setup(x => x.ApplyReaderChange(It.IsAny<ReaderChangeBase>()))
                .ReturnsAsync(ReaderChangeResult.ReaderDoesNotExist);

            var result = await Proxy.CallDirectorServiceAsync(new DeleteReader(new SubscriberConfigurationBuilder().Create()));

            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<ReaderDoesNotExistError>();
        }

        [Fact, IsUnit]
        public async Task When_DirectorServiceIsBusy_Then_DirectorServiceIsBusyErrorReturned()
        {
            _directorServiceMock.Setup(x => x.ApplyReaderChange(It.IsAny<ReaderChangeBase>()))
                .ReturnsAsync(ReaderChangeResult.DirectorIsBusy);

            var result = await Proxy.CallDirectorServiceAsync(new UpdateReader(new SubscriberConfigurationBuilder().Create()));

            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<DirectorServiceIsBusyError>();
        }

        [Fact, IsUnit]
        public async Task When_ReaderCreateFailed_Then_ReaderCreationErrorReturned()
        {
            _directorServiceMock.Setup(x => x.ApplyReaderChange(It.IsAny<ReaderChangeBase>()))
                .ReturnsAsync(ReaderChangeResult.CreateFailed);

            var result = await Proxy.CallDirectorServiceAsync(new UpdateReader(new SubscriberConfigurationBuilder().Create()));

            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<ReaderCreateError>();
        }

        [Fact, IsUnit]
        public async Task When_ReaderDeleteFailed_Then_ReaderDeletionErrorReturned()
        {
            _directorServiceMock.Setup(x => x.ApplyReaderChange(It.IsAny<ReaderChangeBase>()))
                .ReturnsAsync(ReaderChangeResult.DeleteFailed);

            var result = await Proxy.CallDirectorServiceAsync(new UpdateReader(new SubscriberConfigurationBuilder().Create()));

            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<ReaderDeleteError>();
        }

        [Fact, IsUnit]
        public async Task When_DirectorServiceResultNotSet_Then_GenericErrorReturned()
        {
            _directorServiceMock.Setup(x => x.ApplyReaderChange(It.IsAny<ReaderChangeBase>()))
                .ReturnsAsync(ReaderChangeResult.None);

            var result = await Proxy.CallDirectorServiceAsync(new UpdateReader(new SubscriberConfigurationBuilder().Create()));

            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<BusinessError>();
        }
    }
}
