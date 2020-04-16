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
            TestOutputHelper.WriteLine("starting BasicWebHookFlowAuthNoRoutePostVerbTest");
            await Fixture.ExpectTrackedEvent(new WebHookFlowTestEvent(),
                builder => builder
                    .CheckOidcAuthScopes("eda.peterpan.delivery.api.all")
                    .CheckUrlIdSuffixPresent(false)
                    .CheckVerb(HttpMethod.Post));
            TestOutputHelper.WriteLine("BasicWebHookFlowAuthNoRoutePostVerbTest finished");
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
            TestOutputHelper.WriteLine("starting BasicWebHookFlowAuthMatchedRoutePostVerbTest");
            await Fixture.ExpectTrackedEvent(new WebHookFlowRoutedTestEvent() {TenantCode = "GOCAS"},
                builder => builder
                    .CheckOidcAuthScopes("eda.peterpan.delivery.api.all")
                    .CheckUrlIdSuffixPresent(false)
                    .CheckVerb(HttpMethod.Post));
            TestOutputHelper.WriteLine("BasicWebHookFlowAuthMatchedRoutePostVerbTest finished");
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
            TestOutputHelper.WriteLine("starting BasicWebHookFlowAuthUnmatchedRoutePostVerbTest");
            await Fixture.ExpectNoTrackedEvent(new WebHookFlowRoutedTestEvent() { TenantCode = "OTHER" },
                TimeSpan.FromMinutes(3));
            TestOutputHelper.WriteLine("BasicWebHookFlowAuthUnmatchedRoutePostVerbTest finished");
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
            TestOutputHelper.WriteLine("starting BasicCallbackFlowAuthNoRoutePostVerbTest");
            await Fixture.ExpectTrackedEventWithCallback(new CallbackFlowTestEvent(), checks=> checks
                    .CheckOidcAuthScopes("eda.peterpan.delivery.api.all")
                    .CheckUrlIdSuffixPresent(false)
                    .CheckVerb(HttpMethod.Post),
                callbackChecks=> callbackChecks
                    .CheckOidcAuthScopes("eda.peterpan.delivery.api.all")
                    .CheckUrlIdSuffixPresent(false)
                    .CheckIsCallback()
                    .CheckVerb(HttpMethod.Post));
            TestOutputHelper.WriteLine("BasicCallbackFlowAuthNoRoutePostVerbTest finished");
        }
    }
}
