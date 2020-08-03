using CaptainHook.Contract;
using FluentValidation.Validators;
using System;

namespace CaptainHook.Application.Validators.Common
{
    public class UriTransformValidator : PropertyValidator
    {
        public UriTransformValidator()
            : base("{PropertyName} must be valid URI Transform.")
        {
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var rawValue = context.PropertyValue as UriTransformDto;
            // to be completed
            return true;
        }
    }
}
