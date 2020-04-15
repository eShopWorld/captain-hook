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
    public class BasicWebHookFlowAuthNoRoutePostVerb : IntegrationTestBase
    {
        public BasicWebHookFlowAuthNoRoutePostVerb(E2EFlowTestsFixture fixture):base(fixture)
        {
        }

        /// <summary>
        /// Web Hook
        /// POST verb
        /// no routing rules
        /// no model transformation
        /// </summary>
        /// <returns>task</returns>
        [Fact, IsLayer2]
        public async Task BasicWebHookFlowAuthNoRoutePostVerbTest()
        {
            await _fixture.ExpectTrackedEvent(new WebHookFlowTestEvent(),
                builder => builder
                    .CheckOidcAuthScopes("eda.peterpan.delivery.api.all")
                    .CheckUrlIdSuffixPresent(false)
                    .CheckVerb(HttpMethod.Post));
        }
    }

    public class BasicWebHookFlowAuthMatchedRoutePostVerb : IntegrationTestBase
    {
        public BasicWebHookFlowAuthMatchedRoutePostVerb(E2EFlowTestsFixture fixture) : base(fixture)
        {
        }

        /// <summary>
        /// Web Hook
        /// POST verb
        /// with routing rules - matched
        /// no model transformation
        /// </summary>
        /// <returns>task</returns>
        [Fact, IsLayer2]
        public async Task BasicWebHookFlowAuthMatchedRoutePostVerbTest()
        {
            await _fixture.ExpectTrackedEvent(new WebHookFlowRoutedTestEvent() {TenantCode = "GOCAS"},
                builder => builder
                    .CheckOidcAuthScopes("eda.peterpan.delivery.api.all")
                    .CheckUrlIdSuffixPresent(false)
                    .CheckVerb(HttpMethod.Post));
        }
    }

    public class BasicWebHookFlowAuthUnmatchedRoutePostVerb : IntegrationTestBase
    {
        public BasicWebHookFlowAuthUnmatchedRoutePostVerb(E2EFlowTestsFixture fixture) : base(fixture)
        {
        }
        /// <summary>
        /// Web Hook
        /// POST verb
        /// with routing rules - NOT matched
        /// no model transformation
        /// </summary>
        /// <returns>task</returns>
        [Fact, IsLayer2]
        public async Task BasicWebHookFlowAuthUnmatchedRoutePostVerbTest()
        {
            await _fixture.ExpectNoTrackedEvent(new WebHookFlowRoutedTestEvent() { TenantCode = "OTHER" },
                TimeSpan.FromMinutes(3));
        }
    }

    public class BasicCallbackFlowAuthNoRoutePostVerb : IntegrationTestBase
    {
        public BasicCallbackFlowAuthNoRoutePostVerb(E2EFlowTestsFixture fixture) : base(fixture)
        {
        }
        /// <summary>
        /// Web Hook + callback
        /// POST verb
        /// with no route
        /// no model transformation
        /// </summary>
        /// <returns>task</returns>
        [Fact, IsLayer2]
        public async Task BasicWebHookFlowAuthUnmatchedRoutePostVerbTest()
        {
            await _fixture.ExpectTrackedEventWithCallback(new CallbackFlowTestEvent(), checks=> checks
                    .CheckOidcAuthScopes("eda.peterpan.delivery.api.all")
                    .CheckUrlIdSuffixPresent(false)
                    .CheckVerb(HttpMethod.Post),
                callbackChecks=> callbackChecks
                    .CheckOidcAuthScopes("eda.peterpan.delivery.api.all")
                    .CheckUrlIdSuffixPresent(false)
                    .CheckIsCallback()
                    .CheckVerb(HttpMethod.Post));
        }
    }
}
