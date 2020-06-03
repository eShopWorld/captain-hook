namespace CaptainHook.Api.Models
{
    /// <summary>
    /// Defines the result for a refresh config operation
    /// </summary>
    public class RefreshConfigResultDto
    {
        /// <summary>
        /// Number of subscribers added
        /// </summary>
        public int Added { get; set; }

        /// <summary>
        /// Number of subscribers removed
        /// </summary>
        public int Removed { get; set; }

        /// <summary>
        /// Number of subscribers changed
        /// </summary>
        public int Changed { get; set; }
    }
}
