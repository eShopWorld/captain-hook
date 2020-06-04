using CaptainHook.Common.Remoting;

namespace CaptainHook.DirectorService
{
    public class ReloadConfigurationResult: IReloadConfigurationResult
    {
        public int Added { get; }
        public int Removed { get; }
        public int Changed { get; }

        public ReloadConfigurationResult(int added, int removed, int changed)
        {
            Added = added;
            Removed = removed;
            Changed = changed;
        }

    }

}
