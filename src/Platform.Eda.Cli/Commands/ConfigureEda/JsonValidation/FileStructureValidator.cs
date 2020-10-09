using System;
using System.Linq;
using FluentValidation;
using Newtonsoft.Json.Linq;

namespace Platform.Eda.Cli.Commands.ConfigureEda.JsonValidation
{
    public class FileStructureValidator : AbstractValidator<JObject>
    {
        private const string SubscriberName = "subscriberName";

        private const string EventName = "eventName";

        private const string Subscriber = "subscriber";

        private const string Webhooks = "webhooks";

        private const string Endpoints = "endpoints";

        public FileStructureValidator()
        {
            RuleFor(x => x)
                .Cascade(CascadeMode.Stop)
                .Must(o => o.ContainsKey(SubscriberName))
                .WithMessage("File must contain property '{PropertyName}'")
                .Must(o => o[SubscriberName]?.Type == JTokenType.String)
                .WithMessage("File property '{PropertyName}' must be a string")
                .Must(o => FailOnException(() => !string.IsNullOrEmpty(o[SubscriberName]?.Value<string>())))
                .WithMessage("File property '{PropertyName}' cannot be empty")
                .WithName(SubscriberName);
            RuleFor(x => x)
                .Cascade(CascadeMode.Stop)
                .Must(o => o.ContainsKey(EventName))
                .WithMessage("File must contain property '{PropertyName}'")
                .Must(o => o[EventName]?.Type == JTokenType.String)
                .WithMessage("File property '{PropertyName}' must be a string")
                .Must(o => FailOnException(() => !string.IsNullOrEmpty(o[EventName]?.Value<string>())))
                .WithMessage("File property '{PropertyName}' cannot be empty")
                .WithName(EventName);
            RuleFor(x => x)
                .Cascade(CascadeMode.Stop)
                .Must(o => o.ContainsKey(Subscriber))
                .WithMessage("File must contain property '{PropertyName}'")
                .Must(o => o[Subscriber]?.Type == JTokenType.Object)
                .WithMessage("File property '{PropertyName}' must be an object")
                .WithName(Subscriber);
            RuleFor(x => x)
                .Cascade(CascadeMode.Stop)
                .Must(o => FailOnException(() => o[Subscriber]?[Webhooks] != null))
                .WithMessage($"File property '{Subscriber}' must contain property '{Webhooks}'")
                .WithName($"{Subscriber}.{Webhooks}")
                .When(o => o.ContainsKey(Subscriber));
            RuleFor(x => x)
                .Cascade(CascadeMode.Stop)
                .Must(o => FailOnException(() => o[Subscriber]?[Webhooks]?[Endpoints] != null))
                .WithMessage($"File property '{Subscriber}.{Webhooks}' must contain property '{Endpoints}'")
                .Must(o => FailOnException(() => o[Subscriber][Webhooks][Endpoints].Type == JTokenType.Array))
                .WithMessage("File property '{PropertyName}' must be an array")
                .Must(o => FailOnException(() => o[Subscriber][Webhooks][Endpoints].Children().All(c => c.Type == JTokenType.Object)))
                .WithMessage("File property '{PropertyName}' must have endpoints which are objects only")
                .Must(o => FailOnException(() => o[Subscriber][Webhooks][Endpoints].Children().Any()))
                .WithMessage("File property '{PropertyName}' must have at least a single endpoint")
                .WithName($"{Subscriber}.{Webhooks}.{Endpoints}")
                .When(o => o.ContainsKey(Subscriber));
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