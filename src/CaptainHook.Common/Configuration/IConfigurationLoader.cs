namespace CaptainHook.Common.Configuration
{
    public interface IConfigurationLoader
    {
        /// <summary>
        /// Load the configuration from the specified keyvault
        /// </summary>
        /// <param name="keyVaultUri">A Keyvault to load the configuration from</param>
        /// <returns></returns>
        Configuration Load(string keyVaultUri);
    }
}