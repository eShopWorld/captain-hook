﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Application.Infrastructure.DirectorService.Remoting;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using CaptainHook.DirectorService.Infrastructure.Interfaces;
using CaptainHook.DirectorService.ReaderServiceManagement;
using CaptainHook.Tests.Builders;
using CaptainHook.Tests.Services;
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

        private readonly DirectorService.DirectorService _directorService;

        public DirectorServiceTests()
        {
            var context = CustomMockStatefulServiceContextFactory.Create(ServiceNaming.DirectorServiceType, ServiceNaming.DirectorServiceFullName, null);

            _directorService = new DirectorService.DirectorService(context, new Mock<IBigBrother>().Object,
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
            var result = await _directorService.ApplyReaderChange(createRequest);

            result.Should().Be(ReaderChangeResult.Success);
        }

        [Fact, IsUnit]
        public async Task CreateReader_When_ReaderAlreadyExistsForThatConfiguration_Then_ReaderAlreadyExistIsReturned()
        {
            var subscriberConfig = new SubscriberConfigurationBuilder().WithType("testevent").WithSubscriberName("reader").Create();
            var desiredReader = new DesiredReaderDefinition(subscriberConfig);
            _fabricClientMock.Setup(x => x.GetServiceUriListAsync())
                .ReturnsAsync(new List<string> { desiredReader.ServiceNameWithSuffix });

            var createRequest = new CreateReader { Subscriber = subscriberConfig };
            var result = await _directorService.ApplyReaderChange(createRequest);

            result.Should().Be(ReaderChangeResult.ReaderAlreadyExist);
        }

        [Fact, IsUnit]
        public async Task UpdateRequest_When_ReaderExistsInOlderVersion_Then_ReaderIsUpdated()
        {
            var subscriberConfig = new SubscriberConfigurationBuilder().WithType("testevent").WithSubscriberName("reader").Create();
            _fabricClientMock.Setup(x => x.GetServiceUriListAsync())
                .ReturnsAsync(new List<string> { ServiceNaming.EventReaderServiceFullUri("testevent", "reader") });
            _readerServicesManagerMock
                .Setup(x => x.RefreshReadersAsync(It.IsAny<IEnumerable<ReaderChangeInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, RefreshReaderResult>
                {
                    [SubscriberConfiguration.Key(subscriberConfig.EventType, subscriberConfig.SubscriberName)] = RefreshReaderResult.Success
                });

            var updateRequest = new UpdateReader { Subscriber = subscriberConfig };
            var result = await _directorService.ApplyReaderChange(updateRequest);

            result.Should().Be(ReaderChangeResult.Success);
        }

        [Fact, IsUnit]
        public async Task UpdateRequest_When_ReaderDoesNotExist_Then_ReaderDoesNotExistIsReturned()
        {
            var subscriberConfig = new SubscriberConfigurationBuilder().WithType("testevent").WithSubscriberName("reader").Create();
            _fabricClientMock.Setup(x => x.GetServiceUriListAsync())
                .ReturnsAsync(new List<string> { ServiceNaming.EventReaderServiceFullUri("testevent", "other-reader") });

            var updateRequest = new UpdateReader { Subscriber = subscriberConfig };
            var result = await _directorService.ApplyReaderChange(updateRequest);

            result.Should().Be(ReaderChangeResult.ReaderDoesNotExist);
        }

        [Fact, IsUnit]
        public async Task CreateReader_When_CreationFails_Then_CreateFailedErrorIsReturned()
        {
            var subscriberConfig = new SubscriberConfigurationBuilder().WithType("testevent").WithSubscriberName("reader").Create();
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
            var result = await _directorService.ApplyReaderChange(createRequest);

            result.Should().Be(ReaderChangeResult.CreateFailed);
        }

        [Fact, IsUnit]
        public async Task UpdateReader_When_CreationFails_Then_CreateFailedIsReturned()
        {
            var subscriberConfig = new SubscriberConfigurationBuilder().WithType("testevent").WithSubscriberName("reader").Create();
            _fabricClientMock.Setup(x => x.GetServiceUriListAsync())
                .ReturnsAsync(new List<string> { ServiceNaming.EventReaderServiceFullUri("testevent", "reader") });
            _readerServicesManagerMock
                .Setup(x => x.RefreshReadersAsync(It.IsAny<IEnumerable<ReaderChangeInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, RefreshReaderResult>
                {
                    [SubscriberConfiguration.Key(subscriberConfig.EventType, subscriberConfig.SubscriberName)] = RefreshReaderResult.CreateFailed
                });

            var updateRequest = new UpdateReader { Subscriber = subscriberConfig };
            var result = await _directorService.ApplyReaderChange(updateRequest);

            result.Should().Be(ReaderChangeResult.CreateFailed);
        }

        [Fact, IsUnit]
        public async Task UpdateReader_When_DeletionFails_Then_DeleteFailedIsReturned()
        {
            _fabricClientMock
                .Setup(c => c.DeleteServiceAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());
            var subscriberConfig = new SubscriberConfigurationBuilder().WithType("testevent").WithSubscriberName("reader").Create();
            _fabricClientMock.Setup(x => x.GetServiceUriListAsync())
                .ReturnsAsync(new List<string> { ServiceNaming.EventReaderServiceFullUri("testevent", "reader") });
            _readerServicesManagerMock
                .Setup(x => x.RefreshReadersAsync(It.IsAny<IEnumerable<ReaderChangeInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, RefreshReaderResult>
                {
                    [SubscriberConfiguration.Key(subscriberConfig.EventType, subscriberConfig.SubscriberName)] = RefreshReaderResult.DeleteFailed
                });

            var updateReader = new UpdateReader { Subscriber = subscriberConfig };
            var result = await _directorService.ApplyReaderChange(updateReader);

            result.Should().Be(ReaderChangeResult.DeleteFailed);
        }
    }
}