﻿using System.Collections.Generic;
using CaptainHook.Domain.Results;
using Newtonsoft.Json.Linq;

namespace Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor
{
    interface ISubscriberTemplateReplacer
    {
        public OperationResult<string> Replace(TemplateReplacementType replacementType, string fileContent, Dictionary<string, JToken> variablesDictionary);
    }
}