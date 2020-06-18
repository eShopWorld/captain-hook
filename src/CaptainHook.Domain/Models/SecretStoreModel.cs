namespace CaptainHook.Domain.Models
{
    /// <summary>
    /// Secret store model
    /// </summary>
    public class SecretStoreModel
    {
        /// <summary>
        /// Name of the keyvault which holds the secret
        /// </summary>
        public string KeyVaultName { get; }

        /// <summary>
        /// Name of the secret key which holds the actual secret
        /// </summary>
        public string SecretName { get; }
        
        public SecretStoreModel(): this(null, null) { }

        public SecretStoreModel(string keyVaultName, string secretName)
        {
            KeyVaultName = keyVaultName;
            SecretName = secretName;
        }
    }
}
