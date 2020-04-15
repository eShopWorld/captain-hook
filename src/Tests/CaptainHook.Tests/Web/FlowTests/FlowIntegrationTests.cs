using System;
using System.Net.Http;
using System.Threading.Tasks;
using Core.Events.Test;
using Eshopworld.Tests.Core;
using Xunit;
using Xunit.Abstractions;

namespace CaptainHook.Tests.Web.FlowTests
{
    /**
     * E2E integration tests for various identified flows - using peter pan
     */
    public class BasicWebHookFlowAuthNoRoutePostVerb : IntegrationTestBase
    {
        public BasicWebHookFlowAuthNoRoutePostVerb(ITestOutputHelper testOutputHelper, E2EFlowTestsFixture fixture):base(fixture, testOutputHelper)
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
            _testOutputHelper.WriteLine("starting BasicWebHookFlowAuthNoRoutePostVerbTest");
            await _fixture.ExpectTrackedEvent(new WebHookFlowTestEvent(),
                builder => builder
                    .CheckOidcAuthScopes("eda.peterpan.delivery.api.all")
                    .CheckUrlIdSuffixPresent(false)
                    .CheckVerb(HttpMethod.Post));
            _testOutputHelper.WriteLine("BasicWebHookFlowAuthNoRoutePostVerbTest finished");
        }
    }

    public class BasicWebHookFlowAuthMatchedRoutePostVerb : IntegrationTestBase
    {
        public BasicWebHookFlowAuthMatchedRoutePostVerb(ITestOutputHelper testOutputHelper, E2EFlowTestsFixture fixture) : base(fixture, testOutputHelper)
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
            _testOutputHelper.WriteLine("starting BasicWebHookFlowAuthMatchedRoutePostVerbTest");
            await _fixture.ExpectTrackedEvent(new WebHookFlowRoutedTestEvent() {TenantCode = "GOCAS"},
                builder => builder
                    .CheckOidcAuthScopes("eda.peterpan.delivery.api.all")
                    .CheckUrlIdSuffixPresent(false)
                    .CheckVerb(HttpMethod.Post));
            _testOutputHelper.WriteLine("BasicWebHookFlowAuthMatchedRoutePostVerbTest finished");
        }
    }

    public class BasicWebHookFlowAuthUnmatchedRoutePostVerb : IntegrationTestBase
    {
        public BasicWebHookFlowAuthUnmatchedRoutePostVerb(ITestOutputHelper testOutputHelper, E2EFlowTestsFixture fixture) : base(fixture, testOutputHelper)
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
            _testOutputHelper.WriteLine("starting BasicWebHookFlowAuthUnmatchedRoutePostVerbTest");
            await _fixture.ExpectNoTrackedEvent(new WebHookFlowRoutedTestEvent() { TenantCode = "OTHER" },
                TimeSpan.FromMinutes(3));
            _testOutputHelper.WriteLine("BasicWebHookFlowAuthUnmatchedRoutePostVerbTest finished");
        }
    }

    public class BasicCallbackFlowAuthNoRoutePostVerb : IntegrationTestBase
    {
        public BasicCallbackFlowAuthNoRoutePostVerb(ITestOutputHelper testOutputHelper, E2EFlowTestsFixture fixture) : base(fixture, testOutputHelper)
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
        public async Task BasicCallbackFlowAuthNoRoutePostVerbTest()
        {
            _testOutputHelper.WriteLine("starting BasicCallbackFlowAuthNoRoutePostVerbTest");
            await _fixture.ExpectTrackedEventWithCallback(new CallbackFlowTestEvent(), checks=> checks
                    .CheckOidcAuthScopes("eda.peterpan.delivery.api.all")
                    .CheckUrlIdSuffixPresent(false)
                    .CheckVerb(HttpMethod.Post),
                callbackChecks=> callbackChecks
                    .CheckOidcAuthScopes("eda.peterpan.delivery.api.all")
                    .CheckUrlIdSuffixPresent(false)
                    .CheckIsCallback()
                    .CheckVerb(HttpMethod.Post));
            _testOutputHelper.WriteLine("BasicCallbackFlowAuthNoRoutePostVerbTest finished");
        }
    }
}
