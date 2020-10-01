using System.Text;
using Platform.Eda.Cli.Commands.ConfigureEda.Models;

namespace Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor
{
    public interface ISubscriberFileParser
    {
        public PutSubscriberFile ParseFile(string fileName);
    }
}
