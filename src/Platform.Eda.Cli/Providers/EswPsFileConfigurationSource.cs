using System.IO.Abstractions;
using Microsoft.Extensions.Configuration;

namespace Platform.Eda.Cli.Providers
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
