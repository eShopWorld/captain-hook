using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Application.Infrastructure.DirectorService.Remoting;
using CaptainHook.Common;
using CaptainHook.DirectorService.Infrastructure;
using CaptainHook.DirectorService.Infrastructure.Interfaces;
using CaptainHook.DirectorService.ReaderServiceManagement;
using CaptainHook.Tests.Builders;
using Eshopworld.Core;
using Eshopworld.Tests.Core;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Xunit;

namespace CaptainHook.Tests.Director.ReaderServiceManagement
{
    public class ReaderServicesManagerProvisionReaderAsyncTests
    {
        private readonly Mock<IBigBrother> _bigBrotherMock = new Mock<IBigBrother>();
        private readonly Mock<IFabricClientWrapper> _fabricClientMock = new Mock<IFabricClientWrapper>();

        private ReaderServicesManager ReaderServiceManager => new ReaderServicesManager(_fabricClientMock.Object, _bigBrotherMock.Object);

        [Fact, IsUnit]
        public async Task When_ReaderDoesNotExist_Then_ReaderIsCreated()
        {
            var subscriberConfig = new SubscriberConfigurationBuilder().WithType("testevent").WithSubscriberName("reader").Create();
            _fabricClientMock.Setup(x => x.GetServiceUriListAsync())
                .ReturnsAsync(new List<string> { ServiceNaming.EventReaderServiceFullUri("testevent", "other-reader") });

            var result = await ReaderServiceManager.ProvisionReaderAsync(subscriberConfig, CancellationToken.None);

            using (new AssertionScope())
            {
                var desiredReader = new DesiredReaderDefinition(subscriberConfig);
                result.Should().Be(ReaderProvisionResult.Created);
                _fabricClientMock.VerifyFabricClientCreateCalls(desiredReader.ServiceNameWithSuffix);
                _fabricClientMock.VerifyFabricClientDeleteCalls();
                _bigBrotherMock.VerifyServiceCreatedEventPublished(desiredReader.ServiceNameWithSuffix);
                _bigBrotherMock.VerifyServiceDeletedEventPublished();
            }
        }

        [Fact, IsUnit]
        public async Task When_ReaderAlreadyExistsForThatConfiguration_Then_ReaderIsNotCreated()
        {
            var subscriberConfig = new SubscriberConfigurationBuilder().WithType("testevent").WithSubscriberName("reader").Create();
            var desiredReader = new DesiredReaderDefinition(subscriberConfig);
            _fabricClientMock.Setup(x => x.GetServiceUriListAsync())
                .ReturnsAsync(new List<string> { desiredReader.ServiceNameWithSuffix });

            var result = await ReaderServiceManager.ProvisionReaderAsync(subscriberConfig, CancellationToken.None);

            using (new AssertionScope())
            {
                result.Should().Be(ReaderProvisionResult.ReaderAlreadyExists);
                _fabricClientMock.VerifyFabricClientCreateCalls();
                _fabricClientMock.VerifyFabricClientDeleteCalls();
                _bigBrotherMock.VerifyServiceCreatedEventPublished();
                _bigBrotherMock.VerifyServiceDeletedEventPublished();
            }
        }

        [Fact, IsUnit]
        public async Task When_ReaderExistsInOlderVersion_Then_ReaderIsUpdated()
        {
            var subscriberConfig = new SubscriberConfigurationBuilder().WithType("testevent").WithSubscriberName("reader").Create();
            var desiredReader = new DesiredReaderDefinition(subscriberConfig);
            var oldReaderName = ServiceNaming.EventReaderServiceFullUri("testevent", "reader");
            _fabricClientMock.Setup(x => x.GetServiceUriListAsync()).ReturnsAsync(new List<string> { oldReaderName });

            var result = await ReaderServiceManager.ProvisionReaderAsync(subscriberConfig, CancellationToken.None);

            using (new AssertionScope())
            {
                result.Should().Be(ReaderProvisionResult.Updated);
                _fabricClientMock.VerifyFabricClientCreateCalls(desiredReader.ServiceNameWithSuffix);
                _fabricClientMock.VerifyFabricClientDeleteCalls(oldReaderName);
                _bigBrotherMock.VerifyServiceCreatedEventPublished(desiredReader.ServiceNameWithSuffix);
                _bigBrotherMock.VerifyServiceDeletedEventPublished(oldReaderName);
            }
        }

        [Fact, IsUnit]
        public async Task When_CreateReaderFailsForNewSubscriber_Then_ResultHasFailure()
        {
            _fabricClientMock
                .Setup(c => c.CreateServiceAsync(It.IsAny<ServiceCreationDescription>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());
            var subscriberConfig = new SubscriberConfigurationBuilder().WithType("testevent").WithSubscriberName("reader").Create();
            var desiredReader = new DesiredReaderDefinition(subscriberConfig);
            _fabricClientMock
                .Setup(x => x.GetServiceUriListAsync())
                .ReturnsAsync(new List<string> { ServiceNaming.EventReaderServiceFullUri("testevent", "other-reader") });

            var result = await ReaderServiceManager.ProvisionReaderAsync(subscriberConfig, CancellationToken.None);

            using (new AssertionScope())
            {
                result.Should().Be(ReaderProvisionResult.CreateFailed);
                _fabricClientMock.VerifyFabricClientCreateCalls(desiredReader.ServiceNameWithSuffix);
                _fabricClientMock.VerifyFabricClientDeleteCalls();
                _bigBrotherMock.VerifyServiceCreatedEventPublished();
                _bigBrotherMock.VerifyServiceDeletedEventPublished();
            }
        }

        [Fact, IsUnit]
        public async Task When_CreateReaderFailsForUpdatingSubscriber_Then_ResultHasFailure()
        {
            _fabricClientMock
                .Setup(c => c.CreateServiceAsync(It.IsAny<ServiceCreationDescription>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());
            var subscriberConfig = new SubscriberConfigurationBuilder().WithType("testevent").WithSubscriberName("reader").Create();
            var desiredReader = new DesiredReaderDefinition(subscriberConfig);
            var oldReaderName = ServiceNaming.EventReaderServiceFullUri("testevent", "reader");
            _fabricClientMock.Setup(x => x.GetServiceUriListAsync()).ReturnsAsync(new List<string> { oldReaderName });

            var result = await ReaderServiceManager.ProvisionReaderAsync(subscriberConfig, CancellationToken.None);

            using (new AssertionScope())
            {
                result.Should().Be(ReaderProvisionResult.CreateFailed | ReaderProvisionResult.ReaderAlreadyExists);
                _fabricClientMock.VerifyFabricClientCreateCalls(desiredReader.ServiceNameWithSuffix);
                _fabricClientMock.VerifyFabricClientDeleteCalls();
                _bigBrotherMock.VerifyServiceCreatedEventPublished();
                _bigBrotherMock.VerifyServiceDeletedEventPublished();
            }
        }

        [Fact, IsUnit]
        public async Task When_DeleteReaderFailsForUpdatingSubscriber_Then_ResultHasFailure()
        {
            _fabricClientMock
                .Setup(c => c.DeleteServiceAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());
            var subscriberConfig = new SubscriberConfigurationBuilder().WithType("testevent").WithSubscriberName("reader").Create();
            var desiredReader = new DesiredReaderDefinition(subscriberConfig);
            var oldReaderName = ServiceNaming.EventReaderServiceFullUri("testevent", "reader");
            _fabricClientMock.Setup(x => x.GetServiceUriListAsync()).ReturnsAsync(new List<string> { oldReaderName });

            var result = await ReaderServiceManager.ProvisionReaderAsync(subscriberConfig, CancellationToken.None);

            using (new AssertionScope())
            {
                result.Should().Be(ReaderProvisionResult.CreateFailed | ReaderProvisionResult.Created | ReaderProvisionResult.ReaderAlreadyExists);
                _fabricClientMock.VerifyFabricClientCreateCalls(desiredReader.ServiceNameWithSuffix);
                _fabricClientMock.VerifyFabricClientDeleteCalls(oldReaderName);
                _bigBrotherMock.VerifyServiceCreatedEventPublished(desiredReader.ServiceNameWithSuffix);
                _bigBrotherMock.VerifyServiceDeletedEventPublished(oldReaderName);
            }
        }
    }
}