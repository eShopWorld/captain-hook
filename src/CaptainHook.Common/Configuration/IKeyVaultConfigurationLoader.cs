using Microsoft.Extensions.Configuration;

namespace CaptainHook.Common.Configuration
{
    public interface IKeyVaultConfigurationLoader
    {
        /// <summary>
        /// Loads events configuration from KeyVault
        /// </summary>
        /// <returns>Only events configuration</returns>
        IConfigurationRoot Load(string keyVaultUri);
    }
}