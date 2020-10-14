using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CaptainHook.EventHandlerActor.Handlers;
using Eshopworld.Tests.Core;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using RichardSzalay.MockHttp;
using Xunit;
using IHttpClientFactory = CaptainHook.EventHandlerActor.Handlers.IHttpClientFactory;

namespace CaptainHook.EventHandlerActor.Tests
{
    public class HttpSenderTests
    {
        private readonly MockHttpMessageHandler _mockHttp = new MockHttpMessageHandler();

        private readonly Mock<IHttpClientFactory> _factoryMock = new Mock<IHttpClientFactory>();

        [Fact, IsUnit]
        public async Task SendAsync_ValidResponse_HappensOnce()
        {
            // Arrange
            var request = _mockHttp.When("https://eshop.abc")
                .Respond("application/json", "{'prop' : 'abc'}");

            _factoryMock.Setup(f => f.Get(new Uri("https://eshop.abc"), default))
                .Returns(new HttpClient(_mockHttp));

            // Act
            var subject = new HttpSender(_factoryMock.Object);
            var messageResponse = await subject.SendAsync(
                new SendRequest(
                    HttpMethod.Get,
                    new Uri("https://eshop.abc"),
                    new WebHookHeaders(),
                    string.Empty,
                    new[] { TimeSpan.FromMilliseconds(100) }));

            // Assert
            using var _ = new AssertionScope();
            messageResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            _mockHttp.GetMatchCount(request).Should().Be(1);
        }

        [Fact, IsUnit]
        public async Task SendAsync_InvalidResponse_HappensTwoTimes()
        {
            // Arrange
            var request = _mockHttp.When("https://eshop.abc")
                .Respond(HttpStatusCode.InternalServerError);

            _factoryMock.Setup(f => f.Get(new Uri("https://eshop.abc"), default))
                .Returns(new HttpClient(_mockHttp));

            // Act
            var subject = new HttpSender(_factoryMock.Object);
            var messageResponse = await subject.SendAsync(
                new SendRequest(
                    HttpMethod.Get,
                    new Uri("https://eshop.abc"),
                    new WebHookHeaders(),
                    string.Empty,
                    new[] { TimeSpan.FromMilliseconds(100) }));

            // Assert
            using var _ = new AssertionScope();
            messageResponse.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            _mockHttp.GetMatchCount(request).Should().Be(2);
        }
    }
}