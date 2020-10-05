using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Platform.Eda.Cli.Commands.ConfigureEda.OptionsValidation
{
    public class ReplacementParamsDictionaryAttribute : ValidationAttribute
    {
        private static readonly Regex KeyValueRegex = new Regex("^\\S+=\\S+$", RegexOptions.Compiled);

        public ReplacementParamsDictionaryAttribute()
            : base("The value for {0} must be a key and value separated by '='")
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            if (value == null || (value is string str && !KeyValueRegex.IsMatch(str)))
            {
                return new ValidationResult(FormatErrorMessage(context.DisplayName));
            }

            return ValidationResult.Success;
        }
    }
}