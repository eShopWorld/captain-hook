using System;
using System.Threading.Tasks;
using Azure;
using Azure.Security.KeyVault.Secrets;
using Eshopworld.Core;

namespace CaptainHook.Common.Configuration.KeyVault
{
    public class KeyVaultSecretProvider : ISecretProvider
    {
        private readonly IBigBrother _bigBrother;

        private readonly SecretClient _secretClient;

        public KeyVaultSecretProvider(IBigBrother bigBrother, SecretClient secretClient)
        {
            _bigBrother = bigBrother;
            _secretClient = secretClient;
        }
        public Task<string> GetSecretValueAsync(string secretName)
        {
            if (string.IsNullOrEmpty(secretName))
            {
                throw new ArgumentException("Secret name cannot be null or empty.", nameof(secretName));
            }

            return GetSecretValueInternalAsync(secretName);

        }

        private async Task<string> GetSecretValueInternalAsync(string secretName)
        {
            try
            {
                var response = await _secretClient.GetSecretAsync(secretName);
                return response?.Value?.Value;
            }
            catch (RequestFailedException ex)
            {
                var exceptionEvent = ex.ToExceptionEvent<KeyVaultSecretException>();
                exceptionEvent.KeyVault = _secretClient.VaultUri.OriginalString;
                exceptionEvent.SecretName = secretName;

                _bigBrother.Publish(exceptionEvent);

                throw;
            }
        }
    }
}