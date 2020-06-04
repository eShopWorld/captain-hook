namespace CaptainHook.Common.Remoting
{
    public interface IReloadConfigurationResult
    {
        public int Added { get; }
        public int Removed { get; }
        public int Changed { get; }
    }
}
