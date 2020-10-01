using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using Platform.Eda.Cli.Commands.ConfigureEda.Models;

namespace Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor
{
    public class ParamsReplacer
    {
        public PutSubscriberRequest BuildRequest(JObject contentWithoutVars, Dictionary<string, string> pramsDictionary)
        {
            throw new NotImplementedException();
        }
    }
}
