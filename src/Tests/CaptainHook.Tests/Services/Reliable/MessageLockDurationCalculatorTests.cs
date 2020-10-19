using System;
using System.Linq;
using CaptainHook.EventReaderService;
using Eshopworld.Tests.Core;
using FluentAssertions;
using FluentAssertions.Primitives;
using Kusto.Cloud.Platform.Utils;
using Xunit;

namespace CaptainHook.Tests.Services.Reliable
{
    public class MessageLockDurationCalculatorTests
    {
        private readonly MessageLockDurationCalculator _sut = new MessageLockDurationCalculator();

        [Fact]
        [IsUnit]
        public void When_RetrySleepDurationsIsNull_Then_ExceptionIsThrown()
        {
            // Arrange
            void Act() =>_sut.CalculateAsSeconds(TimeSpan.FromSeconds(10), null);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(Act);
        }

        [Theory]
        [IsUnit]
        [InlineData(10, new[] { 10 }, 35)]
        [InlineData(10, new int[] { }, 15)]
        [InlineData(10, new[] { 10, 10 }, 55)]
        [InlineData(10, new[] { 10, 20, 30 }, 105)]
        public void When_CalculateIsInvoked_Then_CorrectValueIsReturned(int httpTimeoutInSeconds, int[] retrySleepDurationsInSeconds, int expectedResult)
        {
            // Arrange
            var httpTimeout = TimeSpan.FromSeconds(httpTimeoutInSeconds);
            var retrySleepDurations = retrySleepDurationsInSeconds
                .Select(x => TimeSpan.FromSeconds(x))
                .ToArray();

            // Act
            var result = _sut.CalculateAsSeconds(httpTimeout, retrySleepDurations);

            // Assert
            result.Should().Be(expectedResult);
        }

        [Theory]
        [IsUnit]
        [InlineData(300, new int[] { })]
        [InlineData(300, new[] { 10 })]
        [InlineData(305, new int[] { })]
        [InlineData(305, new[] { 10 })]
        [InlineData(100, new[] { 50, 50 })]
        [InlineData(100, new[] { 51, 51 })]
        public void When_CalculateIsInvokedWithLargeValues_Then_ReturnValueIsCappedAt300(int httpTimeoutInSeconds, int[] retrySleepDurationsInSeconds)
        {
            // Arrange
            var httpTimeout = TimeSpan.FromSeconds(httpTimeoutInSeconds);
            var retrySleepDurations = retrySleepDurationsInSeconds
                .Select(x => TimeSpan.FromSeconds(x))
                .ToArray();

            // Act
            var result = _sut.CalculateAsSeconds(httpTimeout, retrySleepDurations);

            // Assert
            result.Should().Be(300);
        }
    }
}
