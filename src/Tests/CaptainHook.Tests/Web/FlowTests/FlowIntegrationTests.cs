using System;
using System.Net.Http;
using System.Threading.Tasks;
using Core.Events.Test;
using Eshopworld.Tests.Core;
using Xunit;

namespace CaptainHook.Tests.Web.FlowTests
{
    /**
     * E2E integration tests for various identified flows - using peter pan
     */
    public class FlowIntegrationTests : IntegrationTestBase
    {
        public FlowIntegrationTests(E2EFlowTestsFixture fixture):base(fixture)
        {
            
        }

        [Fact, IsIntegration]
        public async Task RunAllTests()
        {
            await Task.WhenAll(BasicWebHookFlowAuthNoRoutePostVerbTest(), BasicCallbackFlowAuthNoRoutePostVerbTest(),
                BasicWebHookFlowAuthMatchedRoutePostVerbTest(), BasicWebHookFlowAuthUnmatchedRoutePostVerbTest());
        }

        /// <summary>
        /// Web Hook
        /// POST verb
        /// no routing rules
        /// no model transformation
        /// </summary>
        /// <returns>task</returns>
        private Task BasicWebHookFlowAuthNoRoutePostVerbTest()
        {

            return Fixture.ExpectTrackedEvent(new WebHookFlowTestEvent(),
                builder => builder
                    .CheckOidcAuthScopes(PeterPanConsts.PeterPanDeliveryScope)
                    .CheckUrlIdSuffixPresent(false)
                    .CheckVerb(HttpMethod.Post));
        }

        /// <summary>
        /// Web Hook + callback
        /// POST verb
        /// with no route
        /// no model transformation
        /// </summary>
        /// <returns>task</returns>
        private Task BasicCallbackFlowAuthNoRoutePostVerbTest()
        {
            return Fixture.ExpectTrackedEventWithCallback(new CallbackFlowTestEvent(), checks => checks
                    .CheckOidcAuthScopes(PeterPanConsts.PeterPanDeliveryScope)
                    .CheckUrlIdSuffixPresent(false)
                    .CheckVerb(HttpMethod.Post),
                callbackChecks => callbackChecks
                    .CheckOidcAuthScopes(PeterPanConsts.PeterPanDeliveryScope)
                    .CheckUrlIdSuffixPresent(false)
                    .CheckIsCallback()
                    .CheckVerb(HttpMethod.Post));
        }

        /// <summary>
        /// Web Hook
        /// POST verb
        /// with routing rules - matched
        /// no model transformation
        /// </summary>
        /// <returns>task</returns>
        private Task BasicWebHookFlowAuthMatchedRoutePostVerbTest()
        {
            return Fixture.ExpectTrackedEvent(new WebHookFlowRoutedTestEvent() { TenantCode = "GOCAS" },
                builder => builder
                    .CheckOidcAuthScopes(PeterPanConsts.PeterPanDeliveryScope)
                    .CheckUrlIdSuffixPresent(false)
                    .CheckVerb(HttpMethod.Post));
        }

        /// <summary>
        /// Web Hook
        /// POST verb
        /// with routing rules - NOT matched
        /// no model transformation
        /// </summary>
        /// <returns>task</returns>
        private Task BasicWebHookFlowAuthUnmatchedRoutePostVerbTest()
        {
            return Fixture.ExpectNoTrackedEvent(new WebHookFlowRoutedTestEvent() { TenantCode = "OTHER" },
                TimeSpan.FromMinutes(3));
        }
    }
}
