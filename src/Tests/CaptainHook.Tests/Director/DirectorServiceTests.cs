using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Application.Infrastructure.DirectorService.Remoting;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.ServiceBus;
using CaptainHook.DirectorService.Infrastructure.Interfaces;
using CaptainHook.DirectorService.ReaderServiceManagement;
using CaptainHook.Tests.Services;
using CaptainHook.TestsInfrastructure.Builders;
using Eshopworld.Core;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Moq;
using Xunit;

namespace CaptainHook.Tests.Director
{
    public class DirectorServiceTests
    {
        private readonly Mock<IFabricClientWrapper> _fabricClientMock = new Mock<IFabricClientWrapper>();

        private readonly Mock<IReaderServicesManager> _readerServicesManagerMock = new Mock<IReaderServicesManager>();

        private readonly SubscriberConfiguration _defaultSubscriberConfig = new SubscriberConfigurationBuilder().WithType("testevent").WithSubscriberName("reader").Create();

        private readonly DirectorService.DirectorService _directorService;

        private readonly Mock<IServiceBusManager> _serviceBusManagerMock = new Mock<IServiceBusManager>();

        public DirectorServiceTests()
        {
            var context = CustomMockStatefulServiceContextFactory.Create(ServiceNaming.DirectorServiceType, ServiceNaming.DirectorServiceFullName, null);

            _directorService = new DirectorService.DirectorService(context, new Mock<IBigBrother>().Object,
                _readerServicesManagerMock.Object, new Mock<IReaderServiceChangesDetector>().Object,
                _fabricClientMock.Object, new Mock<ISubscriberConfigurationLoader>().Object,
                _serviceBusManagerMock.Object);
        }

        [Fact, IsUnit]
        public async Task CreateReader_When_ReaderDoesNotExist_Then_ReaderIsCreated()
        {
            _fabricClientMock.Setup(x => x.GetServiceUriListAsync())
                .ReturnsAsync(new List<string> { ServiceNaming.EventReaderServiceFullUri("testevent", "other-reader") });
            _readerServicesManagerMock
                .Setup(x => x.RefreshReadersAsync(It.IsAny<IEnumerable<ReaderChangeInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, RefreshReaderResult>
                {
                    [SubscriberConfiguration.Key(_defaultSubscriberConfig.EventType, _defaultSubscriberConfig.SubscriberName)] = RefreshReaderResult.Success
                });

            var createRequest = new CreateReader { Subscriber = _defaultSubscriberConfig };
            var result = await _directorService.ApplyReaderChange(createRequest);

            result.Should().Be(ReaderChangeResult.Success);
        }

        [Fact, IsUnit]
        public async Task CreateReader_When_ReaderAlreadyExistsForThatConfiguration_Then_ReaderAlreadyExistIsReturned()
        {
            var desiredReader = new DesiredReaderDefinition(_defaultSubscriberConfig);
            _fabricClientMock.Setup(x => x.GetServiceUriListAsync())
                .ReturnsAsync(new List<string> { desiredReader.ServiceNameWithSuffix });

            var createRequest = new CreateReader { Subscriber = _defaultSubscriberConfig };
            var result = await _directorService.ApplyReaderChange(createRequest);

            result.Should().Be(ReaderChangeResult.ReaderAlreadyExist);
        }

        [Fact, IsUnit]
        public async Task UpdateReader_When_ReaderExistsInOlderVersion_Then_ReaderIsUpdated()
        {
            _fabricClientMock.Setup(x => x.GetServiceUriListAsync())
                .ReturnsAsync(new List<string> { ServiceNaming.EventReaderServiceFullUri("testevent", "reader") });
            _readerServicesManagerMock
                .Setup(x => x.RefreshReadersAsync(It.IsAny<IEnumerable<ReaderChangeInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, RefreshReaderResult>
                {
                    [SubscriberConfiguration.Key(_defaultSubscriberConfig.EventType, _defaultSubscriberConfig.SubscriberName)] = RefreshReaderResult.Success
                });

            var updateRequest = new UpdateReader { Subscriber = _defaultSubscriberConfig };
            var result = await _directorService.ApplyReaderChange(updateRequest);

            result.Should().Be(ReaderChangeResult.Success);
        }

        [Fact, IsUnit]
        public async Task UpdateReader_When_ReaderExistsInSameVersion_Then_NoChangeNeeded()
        {
            _fabricClientMock.Setup(x => x.GetServiceUriListAsync())
                .ReturnsAsync(new List<string> { new DesiredReaderDefinition(_defaultSubscriberConfig).ServiceNameWithSuffix });

            var updateRequest = new UpdateReader { Subscriber = _defaultSubscriberConfig };
            var result = await _directorService.ApplyReaderChange(updateRequest);

            result.Should().Be(ReaderChangeResult.NoChangeNeeded);
        }

        [Fact, IsUnit]
        public async Task UpdateReader_When_ReaderDoesNotExist_Then_ReaderDoesNotExistIsReturned()
        {
            _fabricClientMock.Setup(x => x.GetServiceUriListAsync())
                .ReturnsAsync(new List<string> { ServiceNaming.EventReaderServiceFullUri("testevent", "other-reader") });

            var updateRequest = new UpdateReader { Subscriber = _defaultSubscriberConfig };
            var result = await _directorService.ApplyReaderChange(updateRequest);

            result.Should().Be(ReaderChangeResult.ReaderDoesNotExist);
        }

        [Fact, IsUnit]
        public async Task CreateReader_When_CreationFails_Then_CreateFailedErrorIsReturned()
        {
            _fabricClientMock
                .Setup(x => x.GetServiceUriListAsync())
                .ReturnsAsync(new List<string> { ServiceNaming.EventReaderServiceFullUri("testevent", "other-reader") });
            _readerServicesManagerMock
                .Setup(x => x.RefreshReadersAsync(It.IsAny<IEnumerable<ReaderChangeInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, RefreshReaderResult>
                {
                    [SubscriberConfiguration.Key(_defaultSubscriberConfig.EventType, _defaultSubscriberConfig.SubscriberName)] = RefreshReaderResult.CreateFailed
                });

            var createRequest = new CreateReader { Subscriber = _defaultSubscriberConfig };
            var result = await _directorService.ApplyReaderChange(createRequest);

            result.Should().Be(ReaderChangeResult.CreateFailed);
        }

        [Fact, IsUnit]
        public async Task UpdateReader_When_CreationFails_Then_CreateFailedIsReturned()
        {
            _fabricClientMock.Setup(x => x.GetServiceUriListAsync())
                .ReturnsAsync(new List<string> { ServiceNaming.EventReaderServiceFullUri("testevent", "reader") });
            _readerServicesManagerMock
                .Setup(x => x.RefreshReadersAsync(It.IsAny<IEnumerable<ReaderChangeInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, RefreshReaderResult>
                {
                    [SubscriberConfiguration.Key(_defaultSubscriberConfig.EventType, _defaultSubscriberConfig.SubscriberName)] = RefreshReaderResult.CreateFailed
                });

            var updateRequest = new UpdateReader { Subscriber = _defaultSubscriberConfig };
            var result = await _directorService.ApplyReaderChange(updateRequest);

            result.Should().Be(ReaderChangeResult.CreateFailed);
        }

        [Fact, IsUnit]
        public async Task UpdateReader_When_DeletionFails_Then_DeleteFailedIsReturned()
        {
            _fabricClientMock
                .Setup(c => c.DeleteServiceAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());
            _fabricClientMock.Setup(x => x.GetServiceUriListAsync())
                .ReturnsAsync(new List<string> { ServiceNaming.EventReaderServiceFullUri("testevent", "reader") });
            _readerServicesManagerMock
                .Setup(x => x.RefreshReadersAsync(It.IsAny<IEnumerable<ReaderChangeInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, RefreshReaderResult>
                {
                    [SubscriberConfiguration.Key(_defaultSubscriberConfig.EventType, _defaultSubscriberConfig.SubscriberName)] = RefreshReaderResult.DeleteFailed
                });

            var updateReader = new UpdateReader { Subscriber = _defaultSubscriberConfig };
            var result = await _directorService.ApplyReaderChange(updateReader);

            result.Should().Be(ReaderChangeResult.DeleteFailed);
        }


        [Fact, IsUnit]
        public async Task DeleteReader_When_ReaderExists_Then_ReaderIsDeleted()
        {
            _fabricClientMock.Setup(x => x.GetServiceUriListAsync())
                .ReturnsAsync(new List<string> { ServiceNaming.EventReaderServiceFullUri("testevent", "reader") });
            _readerServicesManagerMock
                .Setup(x => x.RefreshReadersAsync(It.IsAny<IEnumerable<ReaderChangeInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, RefreshReaderResult>
                {
                    [SubscriberConfiguration.Key(_defaultSubscriberConfig.EventType, _defaultSubscriberConfig.SubscriberName)] = RefreshReaderResult.None
                });

            var deleteRequest = new DeleteReader { Subscriber = _defaultSubscriberConfig };
            var result = await _directorService.ApplyReaderChange(deleteRequest);

            result.Should().Be(ReaderChangeResult.Success);
            _readerServicesManagerMock
                .Verify(x => x.RefreshReadersAsync(It.Is<IEnumerable<ReaderChangeInfo>>(c => c.Single().ChangeType == ReaderChangeType.ToBeRemoved), It.IsAny<CancellationToken>()));
        }

        [Fact, IsUnit]
        public async Task DeleteReader_When_ReaderDoesNotExist_Then_ReaderDoesNotExistIsReturned()
        {
            _fabricClientMock.Setup(x => x.GetServiceUriListAsync())
                .ReturnsAsync(new List<string> { ServiceNaming.EventReaderServiceFullUri("testevent", "other-reader") });

            var deleteRequest = new DeleteReader { Subscriber = _defaultSubscriberConfig };
            var result = await _directorService.ApplyReaderChange(deleteRequest);

            result.Should().Be(ReaderChangeResult.ReaderDoesNotExist);
        }

        [Fact, IsUnit]
        public async Task DeleteReader_When_DeletionFails_Then_DeleteFailedIsReturned()
        {
            _fabricClientMock
                .Setup(c => c.DeleteServiceAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());
            _fabricClientMock.Setup(x => x.GetServiceUriListAsync())
                .ReturnsAsync(new List<string> { ServiceNaming.EventReaderServiceFullUri("testevent", "reader") });
            _readerServicesManagerMock
                .Setup(x => x.RefreshReadersAsync(It.IsAny<IEnumerable<ReaderChangeInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, RefreshReaderResult>
                {
                    [SubscriberConfiguration.Key(_defaultSubscriberConfig.EventType, _defaultSubscriberConfig.SubscriberName)] = RefreshReaderResult.DeleteFailed
                });

            var deleteRequest = new DeleteReader { Subscriber = _defaultSubscriberConfig };
            var result = await _directorService.ApplyReaderChange(deleteRequest);

            result.Should().Be(ReaderChangeResult.DeleteFailed);
        }

        [Fact, IsUnit]
        public async Task DeleteReader_When_ReaderExists_Then_DeleteSubscriptionIsCalled()
        {
            _fabricClientMock.Setup(x => x.GetServiceUriListAsync())
                .ReturnsAsync(new List<string> { ServiceNaming.EventReaderServiceFullUri("testevent", "reader") });
            _readerServicesManagerMock
                .Setup(x => x.RefreshReadersAsync(It.IsAny<IEnumerable<ReaderChangeInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, RefreshReaderResult>
                {
                    [SubscriberConfiguration.Key(_defaultSubscriberConfig.EventType, _defaultSubscriberConfig.SubscriberName)] = RefreshReaderResult.None
                });

            _serviceBusManagerMock
                .Setup(x => x.DeleteSubscriptionAsync(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var deleteRequest = new DeleteReader { Subscriber = _defaultSubscriberConfig };
            var result = await _directorService.ApplyReaderChange(deleteRequest);

            result.Should().Be(ReaderChangeResult.Success);
            _serviceBusManagerMock.Verify(x =>
                x.DeleteSubscriptionAsync("testevent", "reader", CancellationToken.None));
        }
    }
}