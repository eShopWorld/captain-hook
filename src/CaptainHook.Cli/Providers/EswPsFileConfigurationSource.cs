using Microsoft.Extensions.Configuration;
using System.IO.Abstractions;

namespace CaptainHook.Cli.Providers
{
    public class EswPsFileConfigurationSource : IConfigurationSource
    {
        public IFileSystem FileSystem { get; set; }

        public string Path { get; set; }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new EswPsFileConfigurationProvider(Path, FileSystem);
        }
    }
}
