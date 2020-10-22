using System.Collections.Generic;
using System.Linq;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Results;
using Eshopworld.Tests.Core;
using FluentAssertions;
using FluentAssertions.Execution;
using Kusto.Cloud.Platform.Utils;
using Xunit;

namespace CaptainHook.Domain.Tests.Entities
{
    public class WebhooksEntityTests
    {
        private static BasicAuthenticationEntity BasicAuthenticationEntity = new BasicAuthenticationEntity("username", "passwordKeyName");

        [IsUnit, Theory]
        [ClassData(typeof(WebhooksCallbacksDlqHooks))]
        public void SetEndpoint_When_EndpointWithTokenNotInReplaceIsAdded_Then_Fails(WebhooksEntityType type)
        {
            // Arrange
            var uriTransform = new UriTransformEntity(new Dictionary<string, string>()
            {
                ["order"] = "$.Response.Order"
            });
            var entity = new WebhooksEntity(type, "*", new List<EndpointEntity>(), uriTransform);
            var endpoint = new EndpointEntity("http://url.com/{OrderNumber}", BasicAuthenticationEntity, "POST", "*" );

            // Act
            var result = entity.SetEndpoint(endpoint);

            // Assert
            using var _ = new AssertionScope();
            result.IsError.Should().BeTrue();
            result.Error.Failures.Should().HaveCount(1);
            result.Error.Failures.First().As<ValidationFailure>().Message.Should()
                .Be("URI Transform dictionary must contain all the placeholders defined in each URI");
            result.Error.Message.Should().Be("Invalid request");
        }

        [IsUnit, Theory]
        [ClassData(typeof(WebhooksCallbacksDlqHooks))]
        public void SetEndpoint_When_EndpointWithTokenInReplaceIsAdded_Then_Succeeds(WebhooksEntityType type)
        {
            // Arrange
            var uriTransform = new UriTransformEntity(new Dictionary<string, string>()
            {
                ["ordernumber"] = "$.Response.Order"
            });
            var entity = new WebhooksEntity(type, "*", new List<EndpointEntity>(), uriTransform);
            var endpoint = new EndpointEntity("http://url.com/{OrderNumber}", BasicAuthenticationEntity, "POST", "*");

            // Act
            var result = entity.SetEndpoint(endpoint);

            // Assert
            result.IsError.Should().BeFalse();
        }
    }
}
