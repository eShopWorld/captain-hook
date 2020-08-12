using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.DirectorService.Events;
using CaptainHook.DirectorService.Infrastructure.Interfaces;
using CaptainHook.DirectorService.ReaderServiceManagement;
using CaptainHook.Tests.Builders;
using Eshopworld.Core;
using Eshopworld.Tests.Core;
using FluentAssertions.Execution;
using Moq;
using Xunit;

namespace CaptainHook.Tests.Director.ReaderServiceManagement
{
    public class ReaderServicesManagerTests
    {
        private readonly Mock<IBigBrother> _bigBrotherMock = new Mock<IBigBrother>();
        private readonly Mock<IFabricClientWrapper> _fabricClientMock = new Mock<IFabricClientWrapper>();

        private ReaderServicesManager CreateReaderServiceManager()
        {
            return new ReaderServicesManager(_fabricClientMock.Object, _bigBrotherMock.Object);
        }

        [Fact, IsUnit]
        public async Task RefreshReadersAsync_WithNewReadersOnly_OnlyCreatesNewReaders()
        {
            // Arrange
            var readerServiceManager = CreateReaderServiceManager();

            var subscribersToCreate = new[]
            {
                new SubscriberConfigurationBuilder().WithType("testevent").WithCallback().Create(),
                new SubscriberConfigurationBuilder().WithType("testevent").WithSubscriberName("subscriber1").Create(),
                new SubscriberConfigurationBuilder().WithType("testevent.completed").Create(),
            };

            var newReaders = subscribersToCreate.Select(s => new DesiredReaderDefinition(s));
            var changes = newReaders.Select(ReaderChangeInfo.ToBeCreated);

            // Act
            await readerServiceManager.RefreshReadersAsync(changes, CancellationToken.None);

            // Assert
            var expectedCreatedServices = newReaders.Select(r => r.ServiceNameWithSuffix).ToArray();
            using (new AssertionScope())
            {
                _fabricClientMock.VerifyFabricClientCreateCalls(expectedCreatedServices);
                _fabricClientMock.VerifyFabricClientDeleteCalls();

                _bigBrotherMock.VerifyServiceCreatedEventPublished(expectedCreatedServices);
                _bigBrotherMock.VerifyServiceDeletedEventPublished();

                _bigBrotherMock.Verify(b => b.Publish(
                   It.Is<RefreshSubscribersEvent>(m => m.AddedCount == 3 && m.RemovedCount == 0 && m.ChangedCount == 0),
                   It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
            }
        }

        [Fact, IsUnit]
        public async Task RefreshReadersAsync_WithRemovedReadersOnly_OnlyRemovesReaders()
        {
            // Arrange
            var readerServiceManager = CreateReaderServiceManager();

            var existingReaders = new[]
            {
                new ExistingReaderDefinition ("service-1"),
                new ExistingReaderDefinition ("service-2"),
                new ExistingReaderDefinition ("service-3"),
            };

            var changes = existingReaders.Select(ReaderChangeInfo.ToBeRemoved);

            // Act
            await readerServiceManager.RefreshReadersAsync(changes, CancellationToken.None);

            // Assert
            using (new AssertionScope())
            {
                _fabricClientMock.VerifyFabricClientCreateCalls();
                _fabricClientMock.VerifyFabricClientDeleteCalls("service-1", "service-2", "service-3");

                _bigBrotherMock.VerifyServiceCreatedEventPublished();
                _bigBrotherMock.VerifyServiceDeletedEventPublished("service-1", "service-2", "service-3");

                _bigBrotherMock.Verify(b => b.Publish(
                   It.Is<RefreshSubscribersEvent>(m => m.AddedCount == 0 && m.RemovedCount == 3 && m.ChangedCount == 0),
                   It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
            }
        }

        [Fact, IsUnit]
        public async Task RefreshReadersAsync_WithSomeUpdatedReaders_CreatesAndRemovesReaders()
        {
            // Arrange
            var readerServiceManager = CreateReaderServiceManager();

            var subscribersToCreate = new[]
            {
                new SubscriberConfigurationBuilder().WithType("testevent").WithCallback().Create(),
                new SubscriberConfigurationBuilder().WithType("testevent").WithSubscriberName("subscriber1").Create(),
                new SubscriberConfigurationBuilder().WithType("testevent.completed").Create(),
            };

            var newReaders = subscribersToCreate.Select(s => new DesiredReaderDefinition(s));
            var changes = newReaders.Select(r => ReaderChangeInfo.ToBeUpdated(new DesiredReaderDefinition(r.SubscriberConfig), new ExistingReaderDefinition(r.ServiceName)));

            // Act
            await readerServiceManager.RefreshReadersAsync(changes, CancellationToken.None);

            // Assert
            var expectedCreatedServices = newReaders.Select(r => r.ServiceNameWithSuffix).ToArray();
            var expectedDeletedServices = changes.Select(c => c.OldReader.ServiceNameWithSuffix).ToArray();

            using (new AssertionScope())
            {
                _fabricClientMock.VerifyFabricClientCreateCalls(expectedCreatedServices);
                _fabricClientMock.VerifyFabricClientDeleteCalls(expectedDeletedServices);

                _bigBrotherMock.VerifyServiceCreatedEventPublished(expectedCreatedServices);
                _bigBrotherMock.VerifyServiceDeletedEventPublished(expectedDeletedServices);

                _bigBrotherMock.Verify(b => b.Publish(
                   It.Is<RefreshSubscribersEvent>(m => m.AddedCount == 0 && m.RemovedCount == 0 && m.ChangedCount == 3),
                   It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
            }
        }

        [Fact, IsUnit]
        public async Task RefreshReadersAsync_WithAddedAndRemovedReaders_CreatesAndRemovesReaders()
        {
            // Arrange
            var readerServiceManager = CreateReaderServiceManager();

            var subscribersToCreate = new[]
            {
                new SubscriberConfigurationBuilder().WithType("testevent").WithCallback().Create(),
                new SubscriberConfigurationBuilder().WithType("testevent").WithSubscriberName("subscriber1").Create(),
                new SubscriberConfigurationBuilder().WithType("testevent.completed").Create(),
            };

            var newReaders = subscribersToCreate.Select(s => new DesiredReaderDefinition(s));
            var readersToDelete = new[]
            {
                new ExistingReaderDefinition ("service-1"),
                new ExistingReaderDefinition ("service-2"),
                new ExistingReaderDefinition ("service-3"),
            };

            var changes = newReaders.Select(ReaderChangeInfo.ToBeCreated).ToList();
            changes.AddRange(readersToDelete.Select(ReaderChangeInfo.ToBeRemoved));

            // Act
            await readerServiceManager.RefreshReadersAsync(changes, CancellationToken.None);

            // Assert
            var expectedCreatedServices = newReaders.Select(r => r.ServiceNameWithSuffix).ToArray();
            var expectedDeletedServices = readersToDelete.Select(r => r.ServiceNameWithSuffix).ToArray();

            using (new AssertionScope())
            {
                _fabricClientMock.VerifyFabricClientCreateCalls(expectedCreatedServices);
                _fabricClientMock.VerifyFabricClientDeleteCalls(expectedDeletedServices);

                _bigBrotherMock.VerifyServiceCreatedEventPublished(expectedCreatedServices);
                _bigBrotherMock.VerifyServiceDeletedEventPublished(expectedDeletedServices);

                _bigBrotherMock.Verify(b => b.Publish(
                   It.Is<RefreshSubscribersEvent>(m => m.AddedCount == 3 && m.RemovedCount == 3 && m.ChangedCount == 0),
                   It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
            }
        }
    }
}