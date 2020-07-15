using System.Collections.Generic;
using Autofac.Features.Indexed;
using CaptainHook.Common.Configuration;
using CaptainHook.EventHandlerActor.Handlers;
using CaptainHook.EventHandlerActor.Handlers.Requests;
using Eshopworld.Tests.Core;
using Moq;
using Xunit;

namespace CaptainHook.Tests.Services.Actors.Requests
{
    public class RequestBuilderDispatcherTests
    {
        private readonly Mock<IRequestBuilder> _routeAndReplaceBuilder;
        private readonly Mock<IRequestBuilder> _defaultBuilder;
        private readonly RequestBuilderDispatcher _requestBuilderDispatcher;

        public RequestBuilderDispatcherTests()
        {
            _routeAndReplaceBuilder = new Mock<IRequestBuilder>();
            _defaultBuilder = new Mock<IRequestBuilder>();

            var indexMock = new Mock<IIndex<RuleAction, IRequestBuilder>>();
            indexMock.Setup(x => x[RuleAction.RouteAndReplace]).Returns(_routeAndReplaceBuilder.Object);
            indexMock.Setup(x => x[RuleAction.Route]).Returns(_defaultBuilder.Object);

            _requestBuilderDispatcher = new RequestBuilderDispatcher(indexMock.Object);
        }

        [Fact, IsUnit]
        public void BuildUri_ExecutesRouteAndReplace_WhenRouteAndReplaceConfig()
        {
            // Arrange
            var routeAndReplaceConfig = BuildConfig(RuleAction.RouteAndReplace);

            // Act
            _requestBuilderDispatcher.BuildUri(routeAndReplaceConfig, "dummy-payload");

            // Assert
            _routeAndReplaceBuilder.Verify(b => b.BuildUri(routeAndReplaceConfig, "dummy-payload"), Times.Once);
            _defaultBuilder.Verify(b => b.BuildUri(It.IsAny<WebhookConfig>(), It.IsAny<string>()), Times.Never);
        }

        [Theory, IsUnit]
        [InlineData(RuleAction.Route)]
        [InlineData(RuleAction.Add)]
        [InlineData(RuleAction.Replace)]
        public void BuildUri_ExecutesRoute_WhenRouteConfig(RuleAction ruleAction)
        {
            // Arrange
            var routeConfig = BuildConfig(ruleAction);

            // Act
            _requestBuilderDispatcher.BuildUri(routeConfig, "dummy-payload");

            // Assert
            _defaultBuilder.Verify(b => b.BuildUri(routeConfig, "dummy-payload"), Times.Once);
            _routeAndReplaceBuilder.Verify(b => b.BuildUri(It.IsAny<WebhookConfig>(), It.IsAny<string>()), Times.Never);
        }

        [Fact, IsUnit]
        public void GetAuthenticationConfig_ExecutesRouteAndReplace_WhenRouteAndReplaceConfig()
        {
            // Arrange
            var routeAndReplaceConfig = BuildConfig(RuleAction.RouteAndReplace);

            // Act
            _requestBuilderDispatcher.GetAuthenticationConfig(routeAndReplaceConfig, "dummy-payload");

            // Assert
            _routeAndReplaceBuilder.Verify(b => b.GetAuthenticationConfig(routeAndReplaceConfig, "dummy-payload"), Times.Once);
            _defaultBuilder.Verify(b => b.GetAuthenticationConfig(It.IsAny<WebhookConfig>(), It.IsAny<string>()), Times.Never);
        }

        [Theory, IsUnit]
        [InlineData(RuleAction.Route)]
        [InlineData(RuleAction.Add)]
        [InlineData(RuleAction.Replace)]
        public void GetAuthenticationConfig_ExecutesRoute_WhenRouteConfig(RuleAction ruleAction)
        {
            // Arrange
            var routeConfig = BuildConfig(ruleAction);

            // Act
            _requestBuilderDispatcher.GetAuthenticationConfig(routeConfig, "dummy-payload");

            // Assert
            _defaultBuilder.Verify(b => b.GetAuthenticationConfig(routeConfig, "dummy-payload"), Times.Once);
            _routeAndReplaceBuilder.Verify(b => b.GetAuthenticationConfig(It.IsAny<WebhookConfig>(), It.IsAny<string>()), Times.Never);
        }

        private static WebhookConfig BuildConfig(RuleAction ruleAction) => new WebhookConfig
        {
            WebhookRequestRules =
                new List<WebhookRequestRule>
                {
                    new WebhookRequestRule
                    {
                        Destination =
                        {
                            RuleAction = ruleAction
                        },
                        Source =
                        {
                            Replace = new Dictionary<string, string>
                            {
                                { "key", "value" }
                            }
                        }
                    }
                }
        };
    }
}