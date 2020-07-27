using System.Text.RegularExpressions;

namespace CaptainHook.Common.Remoting.Types
{
    /// <summary>
    /// Structure used to represent existing reader service names
    /// </summary>
    public readonly struct ExistingReaderDefinition
    {
        private static readonly Regex RemoveSuffixRegex = new Regex("(|-a|-b|-\\d{14}|-[a-zA-Z0-9]{20,22})$", RegexOptions.Compiled);

        /// <summary>
        /// Bare Reader Service name, without the suffix (non-versioned)
        /// </summary>
        public string ServiceName { get; }

        /// <summary>
        /// Full Reader service name as registered in Service Fabric (versioned)
        /// </summary>
        public string ServiceNameWithSuffix { get; }

        public bool IsValid => ServiceName != null && ServiceNameWithSuffix != null;

        public ExistingReaderDefinition(string serviceNameWithSuffix)
        {
            ServiceName = RemoveSuffixRegex.Replace(serviceNameWithSuffix, string.Empty);
            ServiceNameWithSuffix = serviceNameWithSuffix;
        }

    }
}