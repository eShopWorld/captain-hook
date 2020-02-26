using CaptainHook.Common.Configuration;
using Eshopworld.Tests.Core;
using Xunit;

namespace CaptainHook.Tests.Configuration
{
    public class WebhookConfigTests
    {
        [Theory, IsLayer0]
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
    }
}
