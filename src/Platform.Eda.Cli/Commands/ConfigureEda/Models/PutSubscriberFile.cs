using System.IO;

namespace Platform.Eda.Cli.Commands.ConfigureEda.Models
{
    public class PutSubscriberFile
    {
        public FileInfo File { get; set; }

        public PutSubscriberRequest Request { get; set; }
    }
}