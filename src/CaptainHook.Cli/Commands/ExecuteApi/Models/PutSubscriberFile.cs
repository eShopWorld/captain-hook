using System.IO;

namespace CaptainHook.Cli.Commands.ExecuteApi.Models
{
    public class PutSubscriberFile
    {
        public FileInfo File { get; set; }

        public PutSubscriberRequest Request { get; set; }
    }
}