using System.Linq;
using FluentValidation.Validators;

namespace CaptainHook.Application.Validators.Common
{
    public class HttpVerbValidator : PropertyValidator
    {
        private static readonly string[] _validVerbs = { "POST", "PUT", "GET" };

        public HttpVerbValidator()
            : base($"{{PropertyName}} must be one of allowed HTTP verbs: {string.Join(',', _validVerbs)}.")
        {
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var httpVerb = context.PropertyValue as string;
            return _validVerbs.Contains(httpVerb?.ToUpperInvariant());
        }
    }
}