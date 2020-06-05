namespace CaptainHook.DirectorService
{
    public class ReloadConfigurationResult
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
