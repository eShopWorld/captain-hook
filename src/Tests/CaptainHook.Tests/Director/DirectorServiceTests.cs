using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Application.Infrastructure.DirectorService.Remoting;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.ServiceModels;
using CaptainHook.DirectorService.Infrastructure;
using CaptainHook.DirectorService.Infrastructure.Interfaces;
using CaptainHook.DirectorService.ReaderServiceManagement;
using CaptainHook.Tests.Builders;
using CaptainHook.Tests.Director.ReaderServiceManagement;
using CaptainHook.Tests.Services;
using Eshopworld.Core;
using Eshopworld.Tests.Core;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Xunit;

namespace CaptainHook.Tests.Director
{
    public class DirectorServiceTests
    {
        private readonly Mock<IBigBrother> _bigBrotherMock = new Mock<IBigBrother>();
        private readonly Mock<IFabricClientWrapper> _fabricClientMock = new Mock<IFabricClientWrapper>();
        private readonly Mock<IReaderServicesManager> _readerServicesManagerMock = new Mock<IReaderServicesManager>();

        private DirectorService.DirectorService ReaderServiceManager;

        public DirectorServiceTests()
        {
            var context = CustomMockStatefulServiceContextFactory.Create(ServiceNaming.DirectorServiceType, ServiceNaming.DirectorServiceFullName, null);

            ReaderServiceManager = new DirectorService.DirectorService(context, _bigBrotherMock.Object,
                _readerServicesManagerMock.Object, new Mock<IReaderServiceChangesDetector>().Object,
                _fabricClientMock.Object, new Mock<ISubscriberConfigurationLoader>().Object);
        }

        [Fact, IsUnit]
        public async Task CreateReader_When_ReaderDoesNotExist_Then_ReaderIsCreated()
        {
            var subscriberConfig = new SubscriberConfigurationBuilder().WithType("testevent").WithSubscriberName("reader").Create();
            _fabricClientMock.Setup(x => x.GetServiceUriListAsync())
                .ReturnsAsync(new List<string> { ServiceNaming.EventReaderServiceFullUri("testevent", "other-reader") });
            _readerServicesManagerMock
                .Setup(x => x.RefreshReadersAsync(It.IsAny<IEnumerable<ReaderChangeInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, RefreshReaderResult>
                {
                    [SubscriberConfiguration.Key(subscriberConfig.EventType, subscriberConfig.SubscriberName)] = RefreshReaderResult.Success
                });

            var createRequest = new CreateReader { Subscriber = subscriberConfig };
            var result = await ReaderServiceManager.ApplyReaderChange(createRequest);

            using (new AssertionScope())
            {
                var desiredReader = new DesiredReaderDefinition(subscriberConfig);
                result.Should().Be(ReaderChangeResult.Success);
                //_fabricClientMock.VerifyFabricClientCreateCalls(desiredReader.ServiceNameWithSuffix);
                //_fabricClientMock.VerifyFabricClientDeleteCalls();
                //_bigBrotherMock.VerifyServiceCreatedEventPublished(desiredReader.ServiceNameWithSuffix);
                //_bigBrotherMock.VerifyServiceDeletedEventPublished();
            }
        }

        [Fact, IsUnit]
        public async Task CreateReader_When_ReaderAlreadyExistsForThatConfiguration_Then_ReaderAlreadyExistIsReturned()
        {
            var subscriberConfig = new SubscriberConfigurationBuilder().WithType("testevent").WithSubscriberName("reader").Create();
            var desiredReader = new DesiredReaderDefinition(subscriberConfig);
            _fabricClientMock.Setup(x => x.GetServiceUriListAsync())
                .ReturnsAsync(new List<string> { desiredReader.ServiceNameWithSuffix });
            //_readerServicesManagerMock
            //    .Setup(x => x.RefreshReadersAsync(It.IsAny<IEnumerable<ReaderChangeInfo>>(), It.IsAny<CancellationToken>()))
            //    .ReturnsAsync(new Dictionary<string, bool>
            //    {
            //        [SubscriberConfiguration.Key(subscriberConfig.EventType, subscriberConfig.SubscriberName)] = false
            //    });

            var createRequest = new CreateReader { Subscriber = subscriberConfig };
            var result = await ReaderServiceManager.ApplyReaderChange(createRequest);

            using (new AssertionScope())
            {
                result.Should().Be(ReaderChangeResult.ReaderAlreadyExist);
                //_fabricClientMock.VerifyFabricClientCreateCalls();
                //_fabricClientMock.VerifyFabricClientDeleteCalls();
                //_bigBrotherMock.VerifyServiceCreatedEventPublished();
                //_bigBrotherMock.VerifyServiceDeletedEventPublished();
            }
        }

        [Fact, IsUnit]
        public async Task UpdateRequest_When_ReaderExistsInOlderVersion_Then_ReaderIsUpdated()
        {
            var subscriberConfig = new SubscriberConfigurationBuilder().WithType("testevent").WithSubscriberName("reader").Create();
            var desiredReader = new DesiredReaderDefinition(subscriberConfig);
            var oldReaderName = ServiceNaming.EventReaderServiceFullUri("testevent", "reader");
            _fabricClientMock.Setup(x => x.GetServiceUriListAsync()).ReturnsAsync(new List<string> { oldReaderName });
            _readerServicesManagerMock
                .Setup(x => x.RefreshReadersAsync(It.IsAny<IEnumerable<ReaderChangeInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, RefreshReaderResult>
                {
                    [SubscriberConfiguration.Key(subscriberConfig.EventType, subscriberConfig.SubscriberName)] = RefreshReaderResult.Success
                });

            var updateRequest = new UpdateReader { Subscriber = subscriberConfig };
            var result = await ReaderServiceManager.ApplyReaderChange(updateRequest);

            using (new AssertionScope())
            {
                result.Should().Be(ReaderChangeResult.Success);
                //_fabricClientMock.VerifyFabricClientCreateCalls(desiredReader.ServiceNameWithSuffix);
                //_fabricClientMock.VerifyFabricClientDeleteCalls(oldReaderName);
                //_bigBrotherMock.VerifyServiceCreatedEventPublished(desiredReader.ServiceNameWithSuffix);
                //_bigBrotherMock.VerifyServiceDeletedEventPublished(oldReaderName);
            }
        }

        [Fact, IsUnit]
        public async Task UpdateRequest_When_ReaderDoesNotExist_Then_ReaderDoesNotExistIsReturned()
        {
            var subscriberConfig = new SubscriberConfigurationBuilder().WithType("testevent").WithSubscriberName("reader").Create();
            var desiredReader = new DesiredReaderDefinition(subscriberConfig);
            var oldReaderName = ServiceNaming.EventReaderServiceFullUri("testevent", "reader");
            _fabricClientMock.Setup(x => x.GetServiceUriListAsync()).ReturnsAsync(new List<string> { ServiceNaming.EventReaderServiceFullUri("testevent", "other-reader") });
            //_readerServicesManagerMock
            //    .Setup(x => x.RefreshReadersAsync(It.IsAny<IEnumerable<ReaderChangeInfo>>(), It.IsAny<CancellationToken>()))
            //    .ReturnsAsync(new Dictionary<string, RefreshReaderResult>
            //    {
            //        [SubscriberConfiguration.Key(subscriberConfig.EventType, subscriberConfig.SubscriberName)] = true
            //    });

            var updateRequest = new UpdateReader { Subscriber = subscriberConfig };
            var result = await ReaderServiceManager.ApplyReaderChange(updateRequest);

            using (new AssertionScope())
            {
                result.Should().Be(ReaderChangeResult.ReaderDoesNotExist);
                //_fabricClientMock.VerifyFabricClientCreateCalls(desiredReader.ServiceNameWithSuffix);
                //_fabricClientMock.VerifyFabricClientDeleteCalls(oldReaderName);
                //_bigBrotherMock.VerifyServiceCreatedEventPublished(desiredReader.ServiceNameWithSuffix);
                //_bigBrotherMock.VerifyServiceDeletedEventPublished(oldReaderName);
            }
        }

        [Fact, IsUnit]
        public async Task CreateReader_When_CreationFails_Then_CreateFailedErrorIsReturned()
        {
            //_fabricClientMock
            //    .Setup(c => c.CreateServiceAsync(It.IsAny<ServiceCreationDescription>(), It.IsAny<CancellationToken>()))
            //    .ThrowsAsync(new Exception());
            var subscriberConfig = new SubscriberConfigurationBuilder().WithType("testevent").WithSubscriberName("reader").Create();
            var desiredReader = new DesiredReaderDefinition(subscriberConfig);
            _fabricClientMock
                .Setup(x => x.GetServiceUriListAsync())
                .ReturnsAsync(new List<string> { ServiceNaming.EventReaderServiceFullUri("testevent", "other-reader") });
            _readerServicesManagerMock
                .Setup(x => x.RefreshReadersAsync(It.IsAny<IEnumerable<ReaderChangeInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, RefreshReaderResult>
                {
                    [SubscriberConfiguration.Key(subscriberConfig.EventType, subscriberConfig.SubscriberName)] = RefreshReaderResult.CreateFailed
                });

            var createRequest = new CreateReader { Subscriber = subscriberConfig };
            var result = await ReaderServiceManager.ApplyReaderChange(createRequest);

            using (new AssertionScope())
            {
                result.Should().Be(ReaderChangeResult.CreateFailed);
                //_fabricClientMock.VerifyFabricClientCreateCalls(desiredReader.ServiceNameWithSuffix);
                //_fabricClientMock.VerifyFabricClientDeleteCalls();
                //_bigBrotherMock.VerifyServiceCreatedEventPublished();
                //_bigBrotherMock.VerifyServiceDeletedEventPublished();
            }
        }

        [Fact, IsUnit]
        public async Task UpdateReader_When_CreationFails_Then_CreateFailedIsReturned()
        {
            //_fabricClientMock
            //    .Setup(c => c.CreateServiceAsync(It.IsAny<ServiceCreationDescription>(), It.IsAny<CancellationToken>()))
            //    .ThrowsAsync(new Exception());
            var subscriberConfig = new SubscriberConfigurationBuilder().WithType("testevent").WithSubscriberName("reader").Create();
            var desiredReader = new DesiredReaderDefinition(subscriberConfig);
            var oldReaderName = ServiceNaming.EventReaderServiceFullUri("testevent", "reader");
            _fabricClientMock.Setup(x => x.GetServiceUriListAsync()).ReturnsAsync(new List<string> { oldReaderName });
            _readerServicesManagerMock
                .Setup(x => x.RefreshReadersAsync(It.IsAny<IEnumerable<ReaderChangeInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, RefreshReaderResult>
                {
                    [SubscriberConfiguration.Key(subscriberConfig.EventType, subscriberConfig.SubscriberName)] = RefreshReaderResult.CreateFailed
                });

            var updateRequest = new UpdateReader { Subscriber = subscriberConfig };
            var result = await ReaderServiceManager.ApplyReaderChange(updateRequest);

            using (new AssertionScope())
            {
                result.Should().Be(ReaderChangeResult.CreateFailed);
                //_fabricClientMock.VerifyFabricClientCreateCalls(desiredReader.ServiceNameWithSuffix);
                //_fabricClientMock.VerifyFabricClientDeleteCalls();
                //_bigBrotherMock.VerifyServiceCreatedEventPublished();
                //_bigBrotherMock.VerifyServiceDeletedEventPublished();
            }
        }

        [Fact, IsUnit]
        public async Task UpdateReader_When_DeletionFails_Then_DeleteFailedIsReturned()
        {
            _fabricClientMock
                .Setup(c => c.DeleteServiceAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());
            var subscriberConfig = new SubscriberConfigurationBuilder().WithType("testevent").WithSubscriberName("reader").Create();
            var desiredReader = new DesiredReaderDefinition(subscriberConfig);
            var oldReaderName = ServiceNaming.EventReaderServiceFullUri("testevent", "reader");
            _fabricClientMock.Setup(x => x.GetServiceUriListAsync()).ReturnsAsync(new List<string> { oldReaderName });
            _readerServicesManagerMock
                .Setup(x => x.RefreshReadersAsync(It.IsAny<IEnumerable<ReaderChangeInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, RefreshReaderResult>
                {
                    [SubscriberConfiguration.Key(subscriberConfig.EventType, subscriberConfig.SubscriberName)] = RefreshReaderResult.DeleteFailed
                });

            var updateReader = new UpdateReader { Subscriber = subscriberConfig };
            var result = await ReaderServiceManager.ApplyReaderChange(updateReader);

            using (new AssertionScope())
            {
                result.Should().Be(ReaderChangeResult.DeleteFailed);
                //_fabricClientMock.VerifyFabricClientCreateCalls(desiredReader.ServiceNameWithSuffix);
                //_fabricClientMock.VerifyFabricClientDeleteCalls(oldReaderName);
                //_bigBrotherMock.VerifyServiceCreatedEventPublished(desiredReader.ServiceNameWithSuffix);
                //_bigBrotherMock.VerifyServiceDeletedEventPublished(oldReaderName);
            }
        }
    }
}