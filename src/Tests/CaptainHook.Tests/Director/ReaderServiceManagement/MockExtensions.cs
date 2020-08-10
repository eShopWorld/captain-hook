using System.Threading;
using CaptainHook.DirectorService.Events;
using CaptainHook.DirectorService.Infrastructure;
using CaptainHook.DirectorService.Infrastructure.Interfaces;
using Eshopworld.Core;
using Moq;

namespace CaptainHook.Tests.Director.ReaderServiceManagement
{
    internal static class MockExtensions
    {
        public static void VerifyFabricClientCreateCalls(this Mock<IFabricClientWrapper> fabricClientMock, params string[] serviceNames)
        {
            fabricClientMock.Verify(c => c.CreateServiceAsync(It.IsAny<ServiceCreationDescription>(), It.IsAny<CancellationToken>()), Times.Exactly(serviceNames.Length));

            foreach (var serviceName in serviceNames)
            {
                fabricClientMock.Verify(c => c.CreateServiceAsync(
                        It.Is<ServiceCreationDescription>(m => m.ServiceName == serviceName),
                        It.IsAny<CancellationToken>()),
                    Times.Once);
            }
        }

        public static void VerifyFabricClientDeleteCalls(this Mock<IFabricClientWrapper> fabricClientMock, params string[] serviceNames)
        {
            fabricClientMock.Verify(c => c.DeleteServiceAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(serviceNames.Length));

            foreach (var serviceName in serviceNames)
            {
                fabricClientMock.Verify(c => c.DeleteServiceAsync(It.Is<string>(m => m == serviceName), It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        public static void VerifyServiceCreatedEventPublished(this Mock<IBigBrother> bigBrotherMock, params string[] serviceNames)
        {
            bigBrotherMock.Verify(b => b.Publish(It.IsAny<ReaderServiceCreatedEvent>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Exactly(serviceNames.Length));

            foreach (var serviceName in serviceNames)
            {
                bigBrotherMock.Verify(b => b.Publish(
                        It.Is<ReaderServiceCreatedEvent>(m => m.ReaderName == serviceName),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<int>()),
                    Times.Once);
            }
        }

        public static void VerifyServiceDeletedEventPublished(this Mock<IBigBrother> bigBrotherMock, params string[] serviceNames)
        {
            bigBrotherMock.Verify(b => b.Publish(
                    It.IsAny<ReaderServicesDeletionEvent>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>()),
                Times.Exactly(serviceNames.Length > 0 ? 1 : 0));

            foreach (var serviceName in serviceNames)
            {
                bigBrotherMock.Verify(b => b.Publish(
                        It.Is<ReaderServicesDeletionEvent>(m => m.DeletedNames.Contains(serviceName) || m.Failed.Contains(serviceName)),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<int>()),
                    Times.Once);
            }
        }
    }
}