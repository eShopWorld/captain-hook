﻿using CaptainHook.Common.Configuration;
using CaptainHook.DirectorService.Infrastructure;
using CaptainHook.DirectorService.Infrastructure.Interfaces;
using Eshopworld.Core;
using Eshopworld.Tests.Core;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using System;
using Xunit;

namespace CaptainHook.Tests.Director
{
    public class ReaderServiceNameGeneratorTests
    {
        private IReaderServiceNameGenerator _readerServiceNameGenerator;
        private IDateTimeProvider _dateTimeProvider;
        private SubscriberConfiguration _subscriberConfiguration;

        private const string EventPrefix = "fabric:/CaptainHook/EventReader";
        private const string SubscriberName = "captain-hook";

        private readonly DateTimeOffset BaseTime = new DateTimeOffset(2020, 2, 10, 9, 0, 0, TimeSpan.Zero);

        private const string EventTypeName1 = "customer.product.core.events.productrefreshevent";
        private const string EventTypeName2 = "customer.product.core.events.productreloadevent";
        private string GetMillisecondsAsString(DateTimeOffset time) => (time.Ticks / 10_000).ToString();
        private string FullEventName(string eventTypeName = EventTypeName1, string suffix = "") => $"{EventPrefix}.{eventTypeName}-{SubscriberName}" + (string.IsNullOrEmpty(suffix) ? "" : $"-{suffix}");

        public ReaderServiceNameGeneratorTests()
        {
            _dateTimeProvider = Mock.Of<IDateTimeProvider>();
            _subscriberConfiguration = new SubscriberConfiguration()
            {
                EventType = EventTypeName1,
                SubscriberName = SubscriberName
            };
            _readerServiceNameGenerator = new ReaderServiceNameGenerator(_dateTimeProvider);
        }

        [Fact, IsLayer0]
        public void CanGenerateServiceName()
        {
            Mock.Get(_dateTimeProvider).SetupGet(s => s.UtcNow).Returns(BaseTime);

            var newName = _readerServiceNameGenerator.GenerateNewName(_subscriberConfiguration);

            newName.Should().Be(FullEventName(suffix: GetMillisecondsAsString(BaseTime)));
        }

        [Fact, IsLayer0]
        public void CanGenerateSubsequentServiceNames()
        {
            var serviceList = new string[] { };

            var time1 = BaseTime;
            Mock.Get(_dateTimeProvider).SetupGet(s => s.UtcNow).Returns(time1);
            var newName1 = _readerServiceNameGenerator.GenerateNewName(_subscriberConfiguration);

            var time2 = BaseTime.AddMilliseconds(1);
            Mock.Get(_dateTimeProvider).SetupGet(s => s.UtcNow).Returns(time2);
            var newName2 = _readerServiceNameGenerator.GenerateNewName(_subscriberConfiguration);


            using (new AssertionScope())
            {
                newName1.Should().Be(FullEventName(suffix: GetMillisecondsAsString(time1)));
                newName2.Should().Be(FullEventName(suffix: GetMillisecondsAsString(time2)));
            }
        }

        [Fact, IsLayer0]
        public void CanFindOldServiceNames()
        {
            var serviceList = new[]
            {
                FullEventName(EventTypeName1),
                FullEventName(EventTypeName1, "a"),
                FullEventName(EventTypeName1, "b"),
                FullEventName(EventTypeName1, "12345678901234"),

                FullEventName(EventTypeName2),
                FullEventName(EventTypeName2, "a"),
                FullEventName(EventTypeName2, "b"),
                FullEventName(EventTypeName2, "12345678901234")
            };

            var oldNames = _readerServiceNameGenerator.FindOldNames(_subscriberConfiguration, serviceList);

            oldNames.Should().BeEquivalentTo(
                FullEventName(EventTypeName1),
                FullEventName(EventTypeName1, "a"),
                FullEventName(EventTypeName1, "b"),
                FullEventName(EventTypeName1, "12345678901234"));
        }
    }
}
