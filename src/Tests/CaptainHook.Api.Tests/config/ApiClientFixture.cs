﻿using System;
using CaptainHook.Api.Client;
using EShopworld.Security.Services.Testing.Settings;
using EShopworld.Security.Services.Testing.Token;
using Microsoft.Extensions.Configuration;
using Microsoft.Rest;

namespace CaptainHook.Api.Tests.Config
{
    public class ApiClientFixture
    {
        private Uri CaptainHookTestUri;

        public ApiClientFixture()
        {
            CaptainHookTestUri = new Uri(EnvironmentSettings.Configuration["CaptainHookApiUri"]);
        }

        public ICaptainHookClient GetApiUnauthenticatedClient()
        {
            return new CaptainHookClient(CaptainHookTestUri, AnonymousCredential.Instance);
        }

        public ICaptainHookClient GetApiClient()
        {
            var tb = new RefreshingTokenProviderOptions(
                EnvironmentSettings.StsSettings.Issuer, 
                EnvironmentSettings.StsSettings.Subject, 
                EnvironmentSettings.StsSettings.ClientId, 
                EnvironmentSettings.StsSettings.Scopes, 
                EnvironmentSettings.StsSettings.Audience);
            var token = new TokenCredentialsBuilder(tb).Build();
            return new CaptainHookClient(CaptainHookTestUri, token);
        }

        private class AnonymousCredential : ServiceClientCredentials
        {
            public static ServiceClientCredentials Instance { get; } = new AnonymousCredential();

            private AnonymousCredential()
            {
            }
        }
    }
}