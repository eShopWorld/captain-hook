using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using FluentValidation;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Platform.Eda.Cli.Commands.ConfigureEda.JsonValidation
{
    public class FileVariablesValidator : AbstractValidator<JObject>
    {
        private readonly static Regex VariablesRegex = new Regex("{vars:([^{}:]+)}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly Dictionary<string, JToken> _variables;

        public FileVariablesValidator(Dictionary<string, JToken> variables)
        {
            _variables = variables;

            RuleForEach(x => GetUsedVariables(x).Distinct())
                .Must(HaveVariableDeclared)
                .WithName("Variables")
                .WithMessage(UndeclaredVariableMessage);
        }

        private static string UndeclaredVariableMessage(JObject fileObject, string variableName)
        {
            return $"File must declare variable '{variableName}'.";
        }

        private static IEnumerable<string> GetUsedVariables(JObject obj)
        {
            var toSearch = new Stack<JToken>(obj.Children());
            while (toSearch.Count > 0)
            {
                var inspected = toSearch.Pop();

                if (inspected.Type == JTokenType.Property &&
                    (JProperty)inspected is var property &&
                    property.Value.Type == JTokenType.String &&
                    VariablesRegex.Matches((string)property.Value) is var matches &&
                    matches.Any())
                {
                    foreach (Match match in matches)
                    {
                        yield return match.Groups[1].Value;
                    }
                }

                foreach (var child in inspected)
                {
                    toSearch.Push(child);
                }
            }
        }

        private bool HaveVariableDeclared(string variableName)
        {
            return _variables.Any(x => x.Key.Equals(variableName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
