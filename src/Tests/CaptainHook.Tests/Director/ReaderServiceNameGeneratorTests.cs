using CaptainHook.Common.Configuration;
using CaptainHook.DirectorService.Infrastructure;
using CaptainHook.DirectorService.Infrastructure.Interfaces;
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
        private ReaderServiceNameGenerator _readerServiceNameGenerator;
        private IDateTimeProvider _dateTimeProvider;
        private SubscriberNaming _subscriberNaming;

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
            _subscriberNaming = new SubscriberNaming()
            {
                EventType = EventTypeName1,
                SubscriberName = SubscriberName,
                IsDlqMode = false
            };
            _readerServiceNameGenerator = new ReaderServiceNameGenerator(_dateTimeProvider);
        }

        [Fact, IsLayer0]
        public void CanGenerateServiceName()
        {
            Mock.Get(_dateTimeProvider).SetupGet(s => s.UtcNow).Returns(BaseTime);

            var newName = _readerServiceNameGenerator.GenerateNewName(_subscriberNaming);

            newName.Should().Be(FullEventName(suffix: GetMillisecondsAsString(BaseTime)));
        }

        [Fact, IsLayer0]
        public void CanGenerateSubsequentServiceNames()
        {
            var serviceList = new string[] { };

            var time1 = BaseTime;
            Mock.Get(_dateTimeProvider).SetupGet(s => s.UtcNow).Returns(time1);
            var newName1 = _readerServiceNameGenerator.GenerateNewName(_subscriberNaming);

            var time2 = BaseTime.AddMilliseconds(1);
            Mock.Get(_dateTimeProvider).SetupGet(s => s.UtcNow).Returns(time2);
            var newName2 = _readerServiceNameGenerator.GenerateNewName(_subscriberNaming);


            using (new AssertionScope())
            {
                newName1.Should().Be(FullEventName(suffix: GetMillisecondsAsString(time1)));
                newName2.Should().Be(FullEventName(suffix: GetMillisecondsAsString(time2)));
            }
        }

        [IsLayer0]
        [Theory]
        [InlineData("a")]
        [InlineData("b")]
        [InlineData("12345678901234")]
        //[InlineData("-a")]
        //[InlineData("d")]
        //[InlineData("--")]
        //[InlineData("\\")]
        //[InlineData("1234567890123412345678901234")]
        //[InlineData("a12345678901234")]
        //[InlineData("12345678901234a")]
        public void CanFindOldServiceNames(string suffix)
        {
            var serviceList = new[]
            {
                FullEventName(EventTypeName1),
                FullEventName(EventTypeName1, suffix),

                FullEventName(EventTypeName2),
                FullEventName(EventTypeName2, suffix)
            };

            var oldNames = _readerServiceNameGenerator.FindOldNames(_subscriberNaming, serviceList);

            oldNames.Should().BeEquivalentTo(
                FullEventName(EventTypeName1),
                FullEventName(EventTypeName1, suffix));
        }

        [IsLayer0]
        [Theory]
        [InlineData("-a")]
        [InlineData("d")]
        [InlineData("--")]
        [InlineData("\\")]
        [InlineData("1234567890123412345678901234")]
        [InlineData("a12345678901234")]
        [InlineData("12345678901234a")]
        public void WontFindServiceNames(string suffix)
        {
            var serviceList = new[]
            {
                FullEventName(EventTypeName1),
                FullEventName(EventTypeName1, suffix),

                FullEventName(EventTypeName2),
                FullEventName(EventTypeName2, suffix)
            };

            var oldNames = _readerServiceNameGenerator.FindOldNames(_subscriberNaming, serviceList);

            oldNames.Should().BeEquivalentTo(FullEventName(EventTypeName1));
        }
    }
}
