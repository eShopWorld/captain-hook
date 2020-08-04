using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Core.Events.Test;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Kusto.Data;
using Xunit;

namespace CaptainHook.Tests.Web.FlowTests
{
    /**
     * E2E integration tests for various identified flows - using peter pan
     */
    public class FlowIntegrationTests : IntegrationTestBase
    {
        public FlowIntegrationTests(E2EFlowTestsFixture fixture) : base(fixture)
        {

        }

        /// <summary>
        /// Web Hook
        /// POST verb
        /// no routing rules
        /// no model transformation
        /// </summary>
        /// <returns>task</returns>
        /// <remarks>BasicWebHookFlowAuthNoRoutePostVerbTest</remarks>
        [Fact, IsIntegration]
        public async Task When_PostVerbNoRoutingNoTransformation_Expect_OnlyOneValidEvent()
        {
            // Arrange
            Func<FlowTestPredicateBuilder, FlowTestPredicateBuilder> c = builder =>
                builder.CheckOidcAuthScopes(PeterPanConsts.PeterPanDeliveryScope)
                    .CheckUrlIdSuffixPresent(false)
                    .CheckVerb(HttpMethod.Post);
            var predicate = new FlowTestPredicateBuilder();
            predicate = c.Invoke(predicate);


            // Act
            var testEvent = new WebHookFlowTestEvent();
            var processedEvents = await Fixture.PublishAndPoll(testEvent);

            // Assert
            processedEvents.Should().OnlyContain(m => predicate.AllSubPredicatesMatch(m));
        }

        /// <summary>
        /// Web Hook + callback
        /// POST verb
        /// with no route
        /// no model transformation
        /// </summary>
        /// <returns>task</returns>
        /// <remarks>BasicCallbackFlowAuthNoRoutePostVerbTest</remarks>
        [Fact, IsIntegration]
        public async Task When_PostVerbNoRoutingNoTransformationWithCallback_Expect_ValidEventsWithCallback()
        {
            // Arrange 
            Func<FlowTestPredicateBuilder, FlowTestPredicateBuilder> expectedState = checks => checks
                 .CheckOidcAuthScopes(PeterPanConsts.PeterPanDeliveryScope)
                 .CheckUrlIdSuffixPresent(false)
                 .CheckVerb(HttpMethod.Post);
            var predicate = new FlowTestPredicateBuilder();
            predicate = expectedState.Invoke(predicate);

            Func<FlowTestPredicateBuilder, FlowTestPredicateBuilder> expectedStateCallback = callbackChecks => callbackChecks
                 .CheckOidcAuthScopes(PeterPanConsts.PeterPanDeliveryScope)
                 .CheckUrlIdSuffixPresent(false)
                 .CheckIsCallback()
                 .CheckVerb(HttpMethod.Post);


            var callbackPredicate = new FlowTestPredicateBuilder();
            callbackPredicate = expectedStateCallback.Invoke(callbackPredicate);

            // Act
            var testEvent = new CallbackFlowTestEvent();
            var processedEvents = await Fixture.PublishAndPoll(testEvent, default(TimeSpan), waitForCallback: true);


            // Assert
            CheckIfAnyEventMatchesAllSubpredicates(processedEvents.Where(m => !m.IsCallback), predicate);
            CheckIfAnyEventMatchesAllSubpredicates(processedEvents.Where(m => m.IsCallback), callbackPredicate);
        }

        public static void CheckIfAnyEventMatchesAllSubpredicates(IEnumerable<ProcessedEventModel> processedEvents, FlowTestPredicateBuilder expectedStatePredicate)
        {
            processedEvents.Should().Contain(m => expectedStatePredicate.AllSubPredicatesMatch(m));
        }

        /// <summary>
        /// Web Hook
        /// POST verb
        /// with routing rules - matched
        /// no model transformation
        /// </summary>
        /// <returns>task</returns>
        /// <remarks>BasicWebHookFlowAuthMatchedRoutePostVerbTest</remarks>
        [Fact, IsIntegration]
        public async Task When_PostVerbWithRoutingRulesMatchedNoTransformation_Expect_ValidEventsWithCallback()
        {
            // Arrange
            Func<FlowTestPredicateBuilder, FlowTestPredicateBuilder> expectedState = builder => builder
                    .CheckOidcAuthScopes(PeterPanConsts.PeterPanDeliveryScope)
                    .CheckUrlIdSuffixPresent(false)
                    .CheckVerb(HttpMethod.Post);
            var predicate = new FlowTestPredicateBuilder();
            predicate = expectedState.Invoke(predicate);

            // Act
            var testEvent = new WebHookFlowRoutedTestEvent() { TenantCode = "GOCAS" };
            var processedEvents = await Fixture.PublishAndPoll(testEvent);

            // Assert
            processedEvents.Should().OnlyContain(m => predicate.AllSubPredicatesMatch(m));
        }

        /// <summary>
        /// Web Hook
        /// POST verb
        /// with routing rules - NOT matched
        /// no model transformation
        /// </summary>
        /// <returns>task</returns>
        /// <remarks>BasicWebHookFlowAuthUnmatchedRoutePostVerbTest</remarks>
        [Fact, IsIntegration]
        public async Task When_PostVerbWithRoutingRulesNotMatchNoTransformation_Expect_NoEvents()
        {
            // Arrange
            var testEvent = new WebHookFlowRoutedTestEvent() { TenantCode = "OTHER" };

            // Act
            var processedEvents = await Fixture.PublishAndPoll(testEvent, TimeSpan.FromMinutes(3), false);

            // Assert
            processedEvents.Should().BeNullOrEmpty();
        }
    }
}
