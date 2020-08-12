﻿using System.Collections.Generic;
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

        DirectorServiceProxy Proxy => new DirectorServiceProxy(_mapperMock.Object, _directorServiceMock.Object);

        public DirectorServiceProxyTests()
        {
            _mapperMock.Setup(x => x.MapSubscriberAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(new List<SubscriberConfiguration> { new SubscriberConfiguration() });
        }

        [Fact, IsUnit]
        public async Task When_Success_Then_TrueIsReturned()
        {
            _directorServiceMock.Setup(x => x.ApplyReaderChange(It.IsAny<ReaderChangeBase>()))
                .ReturnsAsync(ReaderChangeResult.Success);

            var result = await Proxy.CreateReaderAsync(new SubscriberBuilder().Create());

            result.IsError.Should().BeFalse();
            result.Data.Should().BeTrue();
        }

        [Fact, IsUnit]
        public async Task When_ReaderAlreadyExists_Then_ErrorIsReturned()
        {
            _directorServiceMock.Setup(x => x.ApplyReaderChange(It.IsAny<ReaderChangeBase>()))
                .ReturnsAsync(ReaderChangeResult.ReaderAlreadyExist);

            var result = await Proxy.CreateReaderAsync(new SubscriberBuilder().Create());

            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<ReaderAlreadyExistsError>();
        }

        [Fact, IsUnit]
        public async Task When_ReaderDoesNotExist_Then_ErrorIsReturned()
        {
            _directorServiceMock.Setup(x => x.ApplyReaderChange(It.IsAny<ReaderChangeBase>()))
                .ReturnsAsync(ReaderChangeResult.ReaderDoesNotExist);

            var result = await Proxy.UpdateReaderAsync(new SubscriberBuilder().Create());

            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<ReaderDoesNotExistError>();
        }

        [Fact, IsUnit]
        public async Task When_DirectorServiceIsBusy_Then_DirectorServiceIsBusyErrorReturned()
        {
            _directorServiceMock.Setup(x => x.ApplyReaderChange(It.IsAny<ReaderChangeBase>()))
                .ReturnsAsync(ReaderChangeResult.DirectorIsBusy);

            var result = await Proxy.CreateReaderAsync(new SubscriberBuilder().Create());

            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<DirectorServiceIsBusyError>();
        }

        [Fact, IsUnit]
        public async Task When_ReaderCreateFailed_Then_ReaderCreationErrorReturned()
        {
            _directorServiceMock.Setup(x => x.ApplyReaderChange(It.IsAny<ReaderChangeBase>()))
                .ReturnsAsync(ReaderChangeResult.CreateFailed);

            var result = await Proxy.CreateReaderAsync(new SubscriberBuilder().Create());

            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<ReaderCreateError>();
        }

        [Fact, IsUnit]
        public async Task When_ReaderDeleteFailed_Then_ReaderDeletionErrorReturned()
        {
            _directorServiceMock.Setup(x => x.ApplyReaderChange(It.IsAny<ReaderChangeBase>()))
                .ReturnsAsync(ReaderChangeResult.DeleteFailed);

            var result = await Proxy.CreateReaderAsync(new SubscriberBuilder().Create());

            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<ReaderDeleteError>();
        }

        [Fact, IsUnit]
        public async Task When_DirectorServiceResultNotSet_Then_GenericErrorReturned()
        {
            _directorServiceMock.Setup(x => x.ApplyReaderChange(It.IsAny<ReaderChangeBase>()))
                .ReturnsAsync(ReaderChangeResult.None);

            var result = await Proxy.CreateReaderAsync(new SubscriberBuilder().Create());

            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<BusinessError>();
        }
    }
}
