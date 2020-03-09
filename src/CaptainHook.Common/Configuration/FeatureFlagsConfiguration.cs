using System;
using System.Collections.Generic;
using System.Linq;
using CaptainHook.Common.Configuration.FeatureFlags;
using JetBrains.Annotations;

namespace CaptainHook.Common.Configuration
{
    public class FeatureFlagsConfiguration
    {
        [UsedImplicitly]
        public ICollection<string> FeatureFlags { get; } = new List<string>();

        public T GetFlag<T>() where T: FeatureFlagBase, new()
        {
            var disabledFlag = new T();
            
            var flag = this.FeatureFlags
                .FirstOrDefault(f => string.Equals(disabledFlag.Identifier, f, StringComparison.OrdinalIgnoreCase));
            var isFlagEnabled = !string.IsNullOrEmpty(flag);
            disabledFlag.SetEnabled(isFlagEnabled);

            return disabledFlag;
        }
    }
}