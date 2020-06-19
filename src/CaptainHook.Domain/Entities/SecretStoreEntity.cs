namespace CaptainHook.Domain.Entities
{
    /// <summary>
    /// Secret store model
    /// </summary>
    public class SecretStoreEntity
    {
        /// <summary>
        /// Name of the keyvault which holds the secret
        /// </summary>
        public string KeyVaultName { get; }

        /// <summary>
        /// Name of the secret key which holds the actual secret
        /// </summary>
        public string SecretName { get; }
        
        public SecretStoreEntity(string keyVaultName, string secretName)
        {
            KeyVaultName = keyVaultName;
            SecretName = secretName;
        }
    }
}
