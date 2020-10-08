using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Platform.Eda.Cli.Commands.ConfigureEda.OptionsValidation
{
    public class EnvironmentValidationAttribute : ValidationAttribute
    {
        private static readonly string[] AllowedEnvironments = { "CI", "TEST", "PREP", "SAND", "PROD" };

        public EnvironmentValidationAttribute()
            : base($"Wrong environment {{0}}. Choose one from: {string.Join(", ", AllowedEnvironments)}")
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            if (value is string s && (AllowedEnvironments.Contains(s, StringComparer.Ordinal) || string.IsNullOrEmpty(s)))
            {
                return ValidationResult.Success;
                
            }

            var message = string.Format(ErrorMessageString, value);
            return new ValidationResult(message);
        }
    }
}