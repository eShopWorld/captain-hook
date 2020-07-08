using System.Net.Http;
using CaptainHook.Common.Configuration;
using Eshopworld.Tests.Core;
using Xunit;

namespace CaptainHook.Tests.Configuration
{
    public class WebhookConfigTests
    {
        [Theory, IsUnit]
        [InlineData("POST")]
        [InlineData("PUT")]
        [InlineData("GET")]
        [InlineData("DELETE")]
        public void CheckHttpVerbProjectsToHttpMethod(string verb)
        {
            var sut = new WebhookConfig
            {
                HttpVerb = verb
            };
            Assert.Equal(sut.HttpMethod.Method, verb);
        }

        [Fact, IsUnit]
        // ReSharper disable once InconsistentNaming
        public void CheckHttpMethodDefaultsToPOST()
        {
            var sut = new WebhookConfig();
            Assert.Equal(sut.HttpMethod, HttpMethod.Post);
        }

        [Theory, IsUnit]
        [InlineData("")]
        [InlineData(null)]
        public void HttpVerbDoesNotProjectNullOrEmptyString(string input)
        {
            var sut = new WebhookConfig() {HttpVerb = input};
            Assert.Equal(sut.HttpMethod, HttpMethod.Post);
        }
    }
}
