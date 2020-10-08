using FluentValidation;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Platform.Eda.Cli.Commands.ConfigureEda.JsonValidation
{
    public class FileReplacementsValidator : AbstractValidator<JObject>
    {
        private static readonly Regex VariablesRegex = new Regex("{vars:([^{}:]+)}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex ParametersRegex = new Regex("{params:([^{}:]+)}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        
        private readonly Dictionary<string, JToken> _variables;
        private readonly Dictionary<string, string> _parameters;

        public FileReplacementsValidator(Dictionary<string, JToken> variables)
        {
            _variables = variables;

            RuleForEach(x => GetUsedReplacements(x, VariablesRegex).Distinct())
                .Must(HaveVariableDeclared)
                .WithName("Variables")
                .WithMessage(UndeclaredVariableMessage);
        }

        public FileReplacementsValidator(Dictionary<string, string> parameters)
        {
            _parameters = parameters;

            RuleForEach(x => GetUsedReplacements(x, ParametersRegex).Distinct())
                .Must(HaveParameterDeclared)
                .WithName("Parameters")
                .WithMessage(UndeclaredParameterMessage);
        }

        private static string UndeclaredVariableMessage(JObject fileObject, string variableName)
        {
            return $"File must declare variable '{variableName}' for the requested environment.";
        }

        private static string UndeclaredParameterMessage(JObject fileObject, string parameterName)
        {
            return $"CLI run must provide parameter '{parameterName}'.";
        }

        private static IEnumerable<string> GetUsedReplacements(JObject obj, Regex replacementRegex)
        {
            var toSearch = new Stack<JToken>(obj.Children());
            while (toSearch.Count > 0)
            {
                var inspected = toSearch.Pop();

                if (inspected.Type == JTokenType.Property &&
                    (JProperty)inspected is var property &&
                    property.Value.Type == JTokenType.String &&
                    replacementRegex.Matches((string)property.Value) is var matches &&
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

        private bool HaveParameterDeclared(string parameterName)
        {
            return _parameters.Any(x => x.Key.Equals(parameterName, StringComparison.OrdinalIgnoreCase));
        }

        private bool HaveVariableDeclared(string variableName)
        {
            return _variables.Any(x => x.Key.Equals(variableName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
