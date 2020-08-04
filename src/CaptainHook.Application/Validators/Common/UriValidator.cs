using System;
using FluentValidation.Validators;

namespace CaptainHook.Application.Validators.Common
{
    public class UriValidator : PropertyValidator
    {
        public UriValidator()
            : base("{PropertyName} must be valid URI.")
        {
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var rawValue = context.PropertyValue as string;
            rawValue = rawValue.Replace("{", String.Empty).Replace("}", String.Empty);
            return Uri.TryCreate(rawValue, UriKind.Absolute, out Uri _);
        }
    }
}