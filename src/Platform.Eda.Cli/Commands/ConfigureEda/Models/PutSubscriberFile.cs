using System.IO.Abstractions;

namespace Platform.Eda.Cli.Commands.ConfigureEda.Models
{
    public class PutSubscriberFile
    {
        public FileInfoBase File { get; set; }

        public PutSubscriberRequest Request { get; set; }
    }
}