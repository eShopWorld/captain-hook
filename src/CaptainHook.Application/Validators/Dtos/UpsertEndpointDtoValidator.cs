using CaptainHook.Contract;
using FluentValidation;

namespace CaptainHook.Application.Validators.Dtos
{
    public class UpsertEndpointDtoValidator : AbstractValidator<EndpointDto>
    {
        private const string SelectorValidationMessage = "Selector has to match the selector identifier or be empty";

        public UpsertEndpointDtoValidator(string upsertSelector)
        {
            Include(new EndpointDtoValidator());
            RuleFor(x => x.Selector).Cascade(CascadeMode.Stop)
                .Empty().When(x => string.IsNullOrWhiteSpace(x.Selector))
                .WithMessage(SelectorValidationMessage)
                .Equal(upsertSelector).When(x => !string.IsNullOrWhiteSpace(x.Selector))
                .WithMessage(SelectorValidationMessage);
        }
    }
}