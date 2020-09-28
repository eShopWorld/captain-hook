using System.IO.Abstractions;

namespace Platform.Eda.Cli.Commands.ConfigureEda.Models
{
    public class PutSubscriberFile
    {
        public FileInfoBase File { get; set; }

        public PutSubscriberRequest Request { get; set; }

        public string Error { get; set; }

        public bool IsError => !string.IsNullOrEmpty(Error);
    }
}