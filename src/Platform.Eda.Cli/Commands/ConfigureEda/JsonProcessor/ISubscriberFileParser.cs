using System.Text;
using CaptainHook.Domain.Results;
using Newtonsoft.Json.Linq;
using Platform.Eda.Cli.Commands.ConfigureEda.Models;

namespace Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor
{
    public interface ISubscriberFileParser
    {
        public OperationResult<JObject> ParseFile(string fileName);
    }
}
