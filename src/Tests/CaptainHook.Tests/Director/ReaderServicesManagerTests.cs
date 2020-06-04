using CaptainHook.Common.Configuration;
using CaptainHook.DirectorService;
using CaptainHook.DirectorService.Utils;
using Eshopworld.Core;
using Eshopworld.Tests.Core;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using System;
using Xunit;

namespace CaptainHook.Tests.Director
{
    public class ReaderServicesManagerTests
    {
        private IReaderServicesManager _readerServicesManager;
        private IDateTimeProvider _dateTimeProvider;
        private SubscriberConfiguration _subscriberConfiguration;

        public ReaderServicesManagerTests()
        {
            var bigBrother = Mock.Of<IBigBrother>();
            var fabricClientWrapper = Mock.Of<IFabricClientWrapper>();
            _dateTimeProvider = Mock.Of<IDateTimeProvider>();
            _subscriberConfiguration = new SubscriberConfiguration()
            {
                EventType = "customer.product.core.events.productrefreshevent",
                SubscriberName = "captain-hook"
            };
            _readerServicesManager = new ReaderServicesManager(fabricClientWrapper, bigBrother, _dateTimeProvider);
        }

        [Fact, IsLayer0]
        public void CanGenerateServiceName()
        {
            var serviceList = new string[] { };

            var time = new DateTimeOffset(637268886191109359, TimeSpan.Zero);
            Mock.Get(_dateTimeProvider).SetupGet(s => s.UtcNow).Returns(time);

            var (newName, _) = _readerServicesManager.FindServiceNames(_subscriberConfiguration, serviceList);

            newName.Should().Be("fabric:/CaptainHook/EventReader.customer.product.core.events.productrefreshevent-captain-hook-1062114810");
        }

        [Fact, IsLayer0]
        public void CanGenerateSubsequentServiceNames()
        {
            var serviceList = new string[] { };

            var time = new DateTimeOffset(637268886191109359, TimeSpan.Zero);
            Mock.Get(_dateTimeProvider).SetupGet(s => s.UtcNow).Returns(time);
            var (newName1, _) = _readerServicesManager.FindServiceNames(_subscriberConfiguration, serviceList);

            time = time.AddMinutes(1);
            Mock.Get(_dateTimeProvider).SetupGet(s => s.UtcNow).Returns(time);
            var (newName2, _) = _readerServicesManager.FindServiceNames(_subscriberConfiguration, serviceList);


            using (new AssertionScope())
            {
                newName1.Should().Be("fabric:/CaptainHook/EventReader.customer.product.core.events.productrefreshevent-captain-hook-1062114810");
                newName2.Should().Be("fabric:/CaptainHook/EventReader.customer.product.core.events.productrefreshevent-captain-hook-1062114811");
            }
        }

        [Fact, IsLayer0]
        public void CanFindOldServiceNames()
        {
            var subscriber = new SubscriberConfiguration()
            {
                EventType = "customer.product.core.events.productrefreshevent",
                SubscriberName = "captain-hook"
            };

            var serviceList = new[]
            {
                "fabric:/CaptainHook/EventReader.customer.product.core.events.productrefreshevent-captain-hook",
                "fabric:/CaptainHook/EventReader.customer.product.core.events.productrefreshevent-captain-hook-a",
                "fabric:/CaptainHook/EventReader.customer.product.core.events.productrefreshevent-captain-hook-b",
                "fabric:/CaptainHook/EventReader.customer.product.core.events.productrefreshevent-captain-hook-1062112701",
                "fabric:/CaptainHook/EventReader.customer.product.core.events.productupdateevent-captain-hook",
                "fabric:/CaptainHook/EventReader.customer.product.core.events.productupdateevent-captain-hook-a",
                "fabric:/CaptainHook/EventReader.customer.product.core.events.productupdateevent-captain-hook-b",
                "fabric:/CaptainHook/EventReader.customer.product.core.events.productupdateevent-captain-hook-1062112701"
            };

            var setTime = new DateTimeOffset(637268886191109359, TimeSpan.Zero);
            Mock.Get(_dateTimeProvider).SetupGet(s => s.UtcNow).Returns(setTime);

            var (_, oldNames) = _readerServicesManager.FindServiceNames(_subscriberConfiguration, serviceList);

            oldNames.Should().Contain(new[]
            {
                "fabric:/CaptainHook/EventReader.customer.product.core.events.productrefreshevent-captain-hook",
                "fabric:/CaptainHook/EventReader.customer.product.core.events.productrefreshevent-captain-hook-a",
                "fabric:/CaptainHook/EventReader.customer.product.core.events.productrefreshevent-captain-hook-b",
                "fabric:/CaptainHook/EventReader.customer.product.core.events.productrefreshevent-captain-hook-1062112701"
            });
        }
    }
}
