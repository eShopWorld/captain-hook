namespace CaptainHook.Repository.Models
{
    public class Authentication
    {
        public string ClientId { get; set; }
        public string KeyVaultName { get; set; }
        public string SecretName { get; set; }
        public string Type { get; set; }
        public string Uri { get; set; }
        public string[] Scopes { get; set; }
    }
}
