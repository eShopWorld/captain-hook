using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace CaptainHook.Cli.Commands.GenerateJson
{
    public abstract class GenerateJsonBase
    {
        private static Regex parseEventRegex = new Regex(@"setConfig\s+'(event--\d+--.*?)'\s+(?:'(.*?)'|(\d))\s+\$KeyVault", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Opens a text file, reads all lines of the file, and then closes the file.
        /// </summary>
        /// <param name="path">The file to open for reading. </param>
        /// <returns>A string containing all lines of the file.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual string ReadText(string path)
        {
            return File.ReadAllText(path);
        }

        //public List<(int, EventHandlerConfig)> ConvertToModels(string input)
        //{


        //    //EventHandlerConfig
        //    return parseEventRegex.Matches(input)
        //        .Cast<Match>()
        //        .Select(match => (
        //            int.Parse(match.Groups[1].Value),
        //            match.Groups[2].Value,
        //            match.Groups[3].Value
        //        )).GroupBy((index, _, _) => index)
        //        .Select(x => (x.Key, ParseEventHandler(x)))
        //        .ToList();
        //}

        //private EventHandlerConfig ParseEventHandler(IEnumerable<EventProperty> input)
        //{
        //    var eventHandlerConfig = new EventHandlerConfig();
        //    eventHandlerConfig.Name = input.SingleOrDefault(x => x.Field.Equals("name", System.StringComparison.OrdinalIgnoreCase))?.Value;
        //    eventHandlerConfig.Type = input.SingleOrDefault(x => x.Field.Equals("type", System.StringComparison.OrdinalIgnoreCase))?.Value;
        //    eventHandlerConfig.WebhookConfig = ParseWebhookConfig(input.Where(x => x.Field.StartsWith("webhookconfig", System.StringComparison.OrdinalIgnoreCase)));
        //    return eventHandlerConfig;
        //}

        //private WebhookConfig ParseWebhookConfig(IEnumerable<EventProperty> input)
        //{
        //    var webhookConfig = new WebhookConfig();
        //    webhookConfig.Name = input.SingleOrDefault(x => x.Field.Equals("webhookconfig--name", System.StringComparison.OrdinalIgnoreCase))?.Value;
        //    webhookConfig.HttpVerb = input.SingleOrDefault(x => x.Field.Equals("webhookconfig--httpverb", System.StringComparison.OrdinalIgnoreCase))?.Value;
        //    var httpMethod = input.SingleOrDefault(x => x.Field.Equals("webhookconfig--httpmethod", System.StringComparison.OrdinalIgnoreCase))?.Value?.ToLowerInvariant();
        //    webhookConfig.HttpMethod = httpMethod switch
        //    {
        //        "get" => HttpMethod.Get,
        //        "post" => HttpMethod.Post,
        //        "put" => HttpMethod.Put,
        //        "delete" => HttpMethod.Delete,
        //        "patch" => HttpMethod.Patch,
        //        "head" => HttpMethod.Head,
        //        "options" => HttpMethod.Options,
        //        "trace" => HttpMethod.Trace,
        //        _ => null
        //    };
        //    var timeout = input.SingleOrDefault(x => x.Field.Equals("webhookconfig--timeout", System.StringComparison.OrdinalIgnoreCase))?.Value;
        //    if(TimeSpan.TryParse(timeout, out TimeSpan parsedTimeout))
        //    {
        //        webhookConfig.Timeout = parsedTimeout;
        //    }
        //    webhookConfig.Uri = input.SingleOrDefault(x => x.Field.Equals("webhookconfig--uri", System.StringComparison.OrdinalIgnoreCase))?.Value;
        //    webhookConfig.AuthenticationConfig = ParseAuthenticationConfig(input.Where(x => x.Field.StartsWith("webhookconfig--authenticationconfig", System.StringComparison.OrdinalIgnoreCase)));
        //    webhookConfig.WebhookRequestRules = ParseWebhookRequestRules(input.Where(x => x.Field.StartsWith("webhookconfig--webhookrequestrules", System.StringComparison.OrdinalIgnoreCase)));
        //    return webhookConfig;
        //}

        //private AuthenticationConfig ParseAuthenticationConfig(IEnumerable<EventProperty> input)
        //{
        //    return new AuthenticationConfig();
        //}

        //private List<WebhookRequestRule> ParseWebhookRequestRules(IEnumerable<EventProperty> input)
        //{
        //    return new List<WebhookRequestRule>();
        //}

        //internal class EventProperty
        //{ 
        //    public int Index { get; set; }
        //    public string Field { get; set; }
        //    public string Value { get; set; }

        //    public static EventProperty FromMatch(Match match)
        //    {
        //        return new EventProperty
        //        {
        //            Index = int.Parse(match.Groups[1].Value),
        //            Field = match.Groups[2].Value,
        //            Value = match.Groups[3].Value
        //        };
        //    }
        //}

    }
}
