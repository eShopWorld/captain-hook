using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CaptainHook.Application.Infrastructure.DirectorService;
using CaptainHook.Application.Infrastructure.DirectorService.Remoting;
using CaptainHook.Application.Infrastructure.Mappers;
using CaptainHook.Common.Configuration;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Errors;
using CaptainHook.Domain.Results;
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
            _mapperMock.Setup(x => x.MapToWebhookAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(new SubscriberConfiguration());

            _mapperMock.Setup(x => x.MapToDlqAsync(It.IsAny<SubscriberEntity>()))
               .ReturnsAsync(new SubscriberConfiguration());
        }

        public static IEnumerable<object[]> Data
        {
            get
            {
                yield return new object[]
                {
                    new Func<DirectorServiceProxy, SubscriberEntity, Task<OperationResult<SubscriberConfiguration>>>((proxy, entity) => proxy.CreateReaderAsync(entity))
                };
                yield return new object[]
                {
                    new Func<DirectorServiceProxy, SubscriberEntity, Task<OperationResult<SubscriberConfiguration>>>((proxy, entity) => proxy.UpdateReaderAsync(entity))
                };
                yield return new object[]
                {
                    new Func<DirectorServiceProxy, SubscriberEntity, Task<OperationResult<SubscriberConfiguration>>>((proxy, entity) => proxy.DeleteReaderAsync(entity))
                };
                yield return new object[]
                {
                    new Func<DirectorServiceProxy, SubscriberEntity, Task<OperationResult<SubscriberConfiguration>>>((proxy, entity) => proxy.CreateDlqReaderAsync(entity))
                };
                yield return new object[]
                {
                    new Func<DirectorServiceProxy, SubscriberEntity, Task<OperationResult<SubscriberConfiguration>>>((proxy, entity) => proxy.UpdateDlqReaderAsync(entity))
                };
                yield return new object[]
                {
                    new Func<DirectorServiceProxy, SubscriberEntity, Task<OperationResult<SubscriberConfiguration>>>((proxy, entity) => proxy.DeleteDlqReaderAsync(entity))
                };
            }
        }

        [Theory, IsUnit]
        [MemberData(nameof(Data))]
        public async Task When_Success_Then_SubscriberConfigurationIsReturned(Func<DirectorServiceProxy, SubscriberEntity, Task<OperationResult<SubscriberConfiguration>>> func)
        {
            _directorServiceMock.Setup(x => x.ApplyReaderChange(It.IsAny<ReaderChangeBase>()))
                .ReturnsAsync(ReaderChangeResult.Success);

            var result = await func(Proxy, new SubscriberBuilder().Create());

            result.IsError.Should().BeFalse();
            result.Data.Should().NotBeNull();
        }

        [Theory, IsUnit]
        [MemberData(nameof(Data))]
        public async Task When_ReaderAlreadyExists_Then_ErrorIsReturned(Func<DirectorServiceProxy, SubscriberEntity, Task<OperationResult<SubscriberConfiguration>>> func)
        {
            _directorServiceMock.Setup(x => x.ApplyReaderChange(It.IsAny<ReaderChangeBase>()))
                .ReturnsAsync(ReaderChangeResult.ReaderAlreadyExist);

            var result = await func(Proxy, new SubscriberBuilder().Create());

            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<ReaderAlreadyExistsError>();
        }

        [Theory, IsUnit]
        [MemberData(nameof(Data))]
        public async Task When_ReaderDoesNotExist_Then_ErrorIsReturned(Func<DirectorServiceProxy, SubscriberEntity, Task<OperationResult<SubscriberConfiguration>>> func)
        {
            _directorServiceMock.Setup(x => x.ApplyReaderChange(It.IsAny<ReaderChangeBase>()))
                .ReturnsAsync(ReaderChangeResult.ReaderDoesNotExist);

            var result = await func(Proxy, new SubscriberBuilder().Create());

            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<ReaderDoesNotExistError>();
        }

        [Theory, IsUnit]
        [MemberData(nameof(Data))]
        public async Task When_DirectorServiceIsBusy_Then_DirectorServiceIsBusyErrorReturned(Func<DirectorServiceProxy, SubscriberEntity, Task<OperationResult<SubscriberConfiguration>>> func)
        {
            _directorServiceMock.Setup(x => x.ApplyReaderChange(It.IsAny<ReaderChangeBase>()))
                .ReturnsAsync(ReaderChangeResult.DirectorIsBusy);

            var result = await func(Proxy, new SubscriberBuilder().Create());

            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<DirectorServiceIsBusyError>();
        }

        [Theory, IsUnit]
        [MemberData(nameof(Data))]
        public async Task When_ReaderCreateFailed_Then_ReaderCreationErrorReturned(Func<DirectorServiceProxy, SubscriberEntity, Task<OperationResult<SubscriberConfiguration>>> func)
        {
            _directorServiceMock.Setup(x => x.ApplyReaderChange(It.IsAny<ReaderChangeBase>()))
                .ReturnsAsync(ReaderChangeResult.CreateFailed);

            var result = await func(Proxy, new SubscriberBuilder().Create());

            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<ReaderCreateError>();
        }

        [Theory, IsUnit]
        [MemberData(nameof(Data))]
        public async Task When_ReaderDeleteFailed_Then_ReaderDeletionErrorReturned(Func<DirectorServiceProxy, SubscriberEntity, Task<OperationResult<SubscriberConfiguration>>> func)
        {
            _directorServiceMock.Setup(x => x.ApplyReaderChange(It.IsAny<ReaderChangeBase>()))
                .ReturnsAsync(ReaderChangeResult.DeleteFailed);

            var result = await func(Proxy, new SubscriberBuilder().Create());

            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<ReaderDeleteError>();
        }

        [Theory, IsUnit]
        [MemberData(nameof(Data))]
        public async Task When_DirectorServiceResultNotSet_Then_GenericErrorReturned(Func<DirectorServiceProxy, SubscriberEntity, Task<OperationResult<SubscriberConfiguration>>> func)
        {
            _directorServiceMock.Setup(x => x.ApplyReaderChange(It.IsAny<ReaderChangeBase>()))
                .ReturnsAsync(ReaderChangeResult.None);

            var result = await func(Proxy, new SubscriberBuilder().Create());

            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<BusinessError>();
        }
    }
}
