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

        public DirectorServiceProxyTests()
        {
            _mapperMock.Setup(x => x.MapSubscriber(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(new List<SubscriberConfiguration> { new SubscriberConfiguration() });
        }

        [Fact, IsUnit]
        public async Task When_CreationSucceed_Then_TrueIsReturned()
        {
            _directorServiceMock.Setup(x => x.RefreshReaderAsync(It.IsAny<SubscriberConfiguration>()))
                .ReturnsAsync(ReaderRefreshResult.Created);

            var proxy = new DirectorServiceProxy(_mapperMock.Object, _directorServiceMock.Object);
            var result = await proxy.CreateReaderAsync(new SubscriberBuilder().Create());

            result.IsError.Should().BeFalse();
            result.Data.Should().BeTrue();
        }

        [Fact, IsUnit]
        public async Task When_UpdateSucceed_Then_TrueIsReturned()
        {
            _directorServiceMock.Setup(x => x.RefreshReaderAsync(It.IsAny<SubscriberConfiguration>()))
                .ReturnsAsync(ReaderRefreshResult.Updated);

            var proxy = new DirectorServiceProxy(_mapperMock.Object, _directorServiceMock.Object);
            var result = await proxy.CreateReaderAsync(new SubscriberBuilder().Create());

            result.IsError.Should().BeFalse();
            result.Data.Should().BeTrue();
        }

        [Fact, IsUnit]
        public async Task When_ReaderAlreadyExist_Then_TrueIsReturned()
        {
            _directorServiceMock.Setup(x => x.RefreshReaderAsync(It.IsAny<SubscriberConfiguration>()))
                .ReturnsAsync(ReaderRefreshResult.ReaderAlreadyExists);

            var proxy = new DirectorServiceProxy(_mapperMock.Object, _directorServiceMock.Object);
            var result = await proxy.CreateReaderAsync(new SubscriberBuilder().Create());

            result.IsError.Should().BeFalse();
            result.Data.Should().BeTrue();
        }

        [Fact, IsUnit]
        public async Task When_DirectorServiceIsBusy_Then_DirectorServiceIsBusyErrorReturned()
        {
            _directorServiceMock.Setup(x => x.RefreshReaderAsync(It.IsAny<SubscriberConfiguration>()))
                .ReturnsAsync(ReaderRefreshResult.DirectorIsBusy);

            var proxy = new DirectorServiceProxy(_mapperMock.Object, _directorServiceMock.Object);
            var result = await proxy.CreateReaderAsync(new SubscriberBuilder().Create());

            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<DirectorServiceIsBusyError>();
        }

        [Fact, IsUnit]
        public async Task When_RefreshFailed_Then_ReaderCreationErrorReturned()
        {
            _directorServiceMock.Setup(x => x.RefreshReaderAsync(It.IsAny<SubscriberConfiguration>()))
                .ReturnsAsync(ReaderRefreshResult.Failure);

            var proxy = new DirectorServiceProxy(_mapperMock.Object, _directorServiceMock.Object);
            var result = await proxy.CreateReaderAsync(new SubscriberBuilder().Create());

            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<ReaderCreationError>();
        }

        [Fact, IsUnit]
        public async Task When_DirectorServiceResultNotSet_Then_GenericErrorReturned()
        {
            _directorServiceMock.Setup(x => x.RefreshReaderAsync(It.IsAny<SubscriberConfiguration>()))
                .ReturnsAsync(ReaderRefreshResult.None);

            var proxy = new DirectorServiceProxy(_mapperMock.Object, _directorServiceMock.Object);
            var result = await proxy.CreateReaderAsync(new SubscriberBuilder().Create());

            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<BusinessError>();
        }
    }
}
