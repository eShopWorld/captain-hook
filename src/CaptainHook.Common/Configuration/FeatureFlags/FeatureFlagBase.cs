namespace CaptainHook.Common.Configuration.FeatureFlags
{
    public abstract class FeatureFlagBase
    {
        protected FeatureFlagBase(string identifier)
        {
            Identifier = identifier;
        }

        public string Identifier { get; }

        public bool IsEnabled { get; private set; }

        internal void SetEnabled(bool isEnabled) => this.IsEnabled = isEnabled;
    }
}