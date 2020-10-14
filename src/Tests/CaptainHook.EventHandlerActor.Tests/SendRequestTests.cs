using System;
using System.Net.Http;
using CaptainHook.EventHandlerActor.Handlers;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Xunit;

namespace CaptainHook.EventHandlerActor.Tests
{
    public class SendRequestTests
    {
        [Fact, IsUnit]
        public void SendRequest_ConstructorNoRetryDurations_ThrowsException()
        {
            // Act
            Func<SendRequest> func = () => new SendRequest(HttpMethod.Get, new Uri("https://eshop.abc"), new WebHookHeaders(), string.Empty, null, default);

            // Assert
            func.Should().Throw<ArgumentNullException>().WithMessage("Retry sleep durations are required *");
        }
    }
}