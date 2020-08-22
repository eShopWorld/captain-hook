namespace CaptainHook.Storage.Cosmos.Models
{
    /// <summary>
    /// Authentication model in cosmos db
    /// </summary>
    internal abstract class AuthenticationSubdocument
    {
        /// <summary>
        /// Authentication type
        /// </summary>
        public abstract string Type { get; }
    }
}
