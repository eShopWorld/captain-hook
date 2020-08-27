using System;
using FluentValidation.Validators;

namespace CaptainHook.Application.Validators.Common
{
    public class UriValidator : PropertyValidator
    {
        private readonly bool _templated;

        public UriValidator(bool templated = true)
            : base("{PropertyName} must be valid URI.")
        {
            _templated = templated;
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var rawValue = context.PropertyValue as string;
            if (_templated)
            {
                rawValue = rawValue.Replace("{", string.Empty).Replace("}", string.Empty);
            }
            return Uri.TryCreate(rawValue, UriKind.Absolute, out Uri _);
        }
    }
}