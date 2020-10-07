using System;
using System.Linq;
using FluentValidation;
using Newtonsoft.Json.Linq;

namespace Platform.Eda.Cli.Commands.ConfigureEda.JsonValidation
{
    public class FileStructureValidator : AbstractValidator<JObject>
    {
        public FileStructureValidator()
        {
            RuleFor(x => x)
                .Must(o => o.ContainsKey("subscriberName"))
                .WithMessage("'subscriberName' is required")
                .Must(o => o["subscriberName"]?.Type == JTokenType.String)
                .WithMessage("'subscriberName' must be a string")
                .Must(o => FailOnException(() => !string.IsNullOrEmpty(o["subscriberName"]?.Value<string>())))
                .WithMessage("'subscriberName' cannot be empty")
                .WithName("subscriberName");
            RuleFor(x => x)
                .Must(o => o.ContainsKey("eventName"))
                .WithMessage("'eventName' is required")
                .Must(o => o["eventName"]?.Type == JTokenType.String)
                .WithMessage("'eventName' must be a string")
                .Must(o => FailOnException(() => !string.IsNullOrEmpty(o["eventName"]?.Value<string>())))
                .WithMessage("'eventName' cannot be empty")
                .WithName("eventName");
            RuleFor(x => x)
                .Must(o => o.ContainsKey("subscriber"))
                .WithMessage("'subscriber' is required")
                .Must(o => o["subscriber"]?.Type == JTokenType.Object)
                .WithMessage("'subscriber' must be an object")
                .WithName("subscriber");
            RuleFor(x => x)
                .Must(o => FailOnException(() => o["subscriber"]?["webhooks"] != null))
                .WithMessage("'subscriber' must contain 'webhooks'")
                .WithName("subscriber.webhooks");
            RuleFor(x => x)
                .Must(o => FailOnException(() => o["subscriber"]?["webhooks"]?["endpoints"] != null))
                .WithMessage("'subscriber.webhooks' must contain 'endpoints'")
                .Must(o => FailOnException(() => o["subscriber"]["webhooks"]["endpoints"].Type == JTokenType.Array))
                .WithMessage("'subscriber.webhooks.endpoints' must be an array")
                .Must(o => FailOnException(() => o["subscriber"]["webhooks"]["endpoints"].Children().All(c => c.Type == JTokenType.Object)))
                .WithMessage("'subscriber.webhooks.endpoints' must have endpoints which are objects only")
                .Must(o => FailOnException(() => o["subscriber"]["webhooks"]["endpoints"].Children().Any()))
                .WithMessage("'subscriber.webhooks.endpoints' must have at least a single endpoint")
                .WithName("subscriber.webhooks.endpoints");
        }

        private static bool FailOnException(Func<bool> func)
        {
            try
            {
                return func();
            }
            catch
            {
                return false;
            }
        }
    }
}