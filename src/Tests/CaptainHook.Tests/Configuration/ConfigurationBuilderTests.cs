using System;
using System.Collections.Generic;
using System.Linq;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.Nasty;
using Eshopworld.Tests.Core;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Newtonsoft.Json;
using Xunit;

namespace CaptainHook.Tests.Configuration
{
    public class Test
    {
        [IsLayer0]
        [Fact]
        public void TestTest()
        {
            var payload = new HttpResponseDto
            {
                OrderCode = Guid.NewGuid(),
                Content = string.Empty,
                StatusCode = 200
            };
            JsonConvert.SerializeObject(payload);
        }
    }
}
