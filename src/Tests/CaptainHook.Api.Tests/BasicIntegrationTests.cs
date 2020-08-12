using Eshopworld.Tests.Core;
using EShopworld.Security.Services.Testing.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Xunit;

namespace CaptainHook.Tests.Web.FlowTests
{
    public class BasicIntegrationTests
    {
        // this is going to be the CaptainHook client generated with Autorest later
        
        HttpClient CaptainHookClient;

        public BasicIntegrationTests()
        {
            CaptainHookClient = new HttpClient();
            CaptainHookClient.BaseAddress = new Uri(EnvironmentSettings.Configuration["CaptainHookTestUri"]);
        }

        public TokenCredentials GetAuthToken()
        {
            // same as the one in ApiFixture
            return default;
        }

        //public async Task GetSubscribers_withoutauth()
        //{
        //    // seems to be covered pretty neatly in the other tests
        //    HttpRequest req = new HttpRequest("", )
            
        //    this.CaptainHookClient.GetAsync()
        //}
    }
}
