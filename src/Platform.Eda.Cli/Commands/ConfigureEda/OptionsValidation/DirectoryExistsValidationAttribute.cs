using System.ComponentModel.DataAnnotations;
using System.IO;

namespace Platform.Eda.Cli.Commands.ConfigureEda.OptionsValidation
{
    public class DirectoryExistsValidationAttribute : ValidationAttribute
    {
        public DirectoryExistsValidationAttribute()
            : base("Directory '{0}' either does not exist, or it is not accessible.")
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            if (value is string s && Directory.Exists(s))
                return ValidationResult.Success;

            return new ValidationResult(string.Format(ErrorMessageString, value));
        }
    }
}