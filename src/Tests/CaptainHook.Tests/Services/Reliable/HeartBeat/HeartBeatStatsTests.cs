using System;
using System.Fabric;
using CaptainHook.Common;
using CaptainHook.EventReaderService.HeartBeat;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Xunit;

namespace CaptainHook.Tests.Services.Reliable.HeartBeat
{
    public class HeartBeatStatsTests
    {
        private readonly HeartBeatStats _heartBeat = new HeartBeatStats(true);

        private readonly StatefulServiceContext _context;

        public HeartBeatStatsTests()
        {
            _context = CustomMockStatefulServiceContextFactory.Create(
                ServiceNaming.EventReaderServiceType,
                ServiceNaming.EventReaderServiceFullUri("test.type", "subA"),
                null,
                replicaId: (new Random(int.MaxValue)).Next());
        }

        [Fact, IsUnit]
        public void ReportInFlight_ReflectedInTelemetry()
        {
            // Act
            _heartBeat.ReportInFlight(5, 15);
            var telemetryEvent = _heartBeat.ToTelemetryEvent(_context);

            // Assert
            var expected = new ReaderHeartBeatEvent(_context)
            {
                NumberOfMessagesInFlight = 5,
                NumberOfAvailableHandlers = 15
            };
            telemetryEvent.Should().BeEquivalentTo(expected);
        }

        [Fact, IsUnit]
        public void ReportMessagesRead_Above0_ReflectedInTelemetry()
        {
            // Act
            _heartBeat.ReportMessagesRead(5);
            var telemetryEvent = _heartBeat.ToTelemetryEvent(_context);

            // Assert
            var expected = new ReaderHeartBeatEvent(_context)
            {
                NumberOfMessagesReadLastTime = 5,
                NumberOfMessagesReadSinceLastHeartBeat = 5,
                NumberOfTimesNoMessagesReadSinceLastHeartBeat = 0
            };
            telemetryEvent.Should().BeEquivalentTo(expected, config => config
                .Using<DateTime?>(context => context.Subject.Should().NotBeNull())
                .WhenTypeIs<DateTime?>());
        }

        [Fact, IsUnit]
        public void ReportMessagesRead_Above0_Then_Above0_ReflectedInTelemetry()
        {
            // Act
            _heartBeat.ReportMessagesRead(5);
            _heartBeat.ReportMessagesRead(6);
            var telemetryEvent = _heartBeat.ToTelemetryEvent(_context);

            // Assert
            var expected = new ReaderHeartBeatEvent(_context)
            {
                NumberOfMessagesReadLastTime = 6,
                NumberOfMessagesReadSinceLastHeartBeat = 11,
                NumberOfTimesNoMessagesReadSinceLastHeartBeat = 0
            };
            telemetryEvent.Should().BeEquivalentTo(expected, config => config
                .Using<DateTime?>(context => context.Subject.Should().NotBeNull())
                .WhenTypeIs<DateTime?>());
        }

        [Fact, IsUnit]
        public void ReportMessagesRead_Above0_Then_0_Then_0_ReflectedInTelemetry()
        {
            // Act
            _heartBeat.ReportMessagesRead(5);
            _heartBeat.ReportMessagesRead(0);
            _heartBeat.ReportMessagesRead(0);
            var telemetryEvent = _heartBeat.ToTelemetryEvent(_context);

            // Assert
            var expected = new ReaderHeartBeatEvent(_context)
            {
                NumberOfMessagesReadLastTime = 0,
                NumberOfMessagesReadSinceLastHeartBeat = 5,
                NumberOfTimesNoMessagesReadSinceLastHeartBeat = 2
            };
            telemetryEvent.Should().BeEquivalentTo(expected, config => config
                .Using<DateTime?>(context => context.Subject.Should().NotBeNull())
                .WhenTypeIs<DateTime?>());
        }

        [Fact, IsUnit]
        public void ToTelemetryEvent_CalledMultipleTimes_ResetsAggregatesBetweenCalls()
        {
            // Act
            _heartBeat.ReportInFlight(5, 15);
            _heartBeat.ReportMessagesRead(5);
            var _ = _heartBeat.ToTelemetryEvent(_context);
            var telemetryEvent2 = _heartBeat.ToTelemetryEvent(_context);

            // Assert
            var expected = new ReaderHeartBeatEvent(_context)
            {
                NumberOfMessagesInFlight = 5,
                NumberOfAvailableHandlers = 15,
                NumberOfMessagesReadLastTime = 5,
                NumberOfMessagesReadSinceLastHeartBeat = 0,
                NumberOfTimesNoMessagesReadSinceLastHeartBeat = 0
            };
            telemetryEvent2.Should().BeEquivalentTo(expected, config => config
                .Using<DateTime?>(context => context.Subject.Should().NotBeNull())
                .WhenTypeIs<DateTime?>());
        }

        [Fact, IsUnit]
        public void HeartBeatDisabled_ToTelemetryEvent_ThrowsException()
        {
            // Act
            var heartBeatDisabled = new HeartBeatStats(false);
            Func<ReaderHeartBeatEvent> toTelemetryAction = () => heartBeatDisabled.ToTelemetryEvent(_context);

            // Assert
            toTelemetryAction.Should().Throw<NotSupportedException>().Which.Message
                .Should().Be("Heart Beat monitoring is disabled for service: fabric:/CaptainHook/EventReader.test.type-subA");
        }
    }
}