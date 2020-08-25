using CaptainHook.Contract;
using FluentValidation;

namespace CaptainHook.Application.Validators.Dtos
{
    public class UpsertEndpointDtoValidator : AbstractValidator<EndpointDto>
    {
        private const string SelectorValidationMessage = "Selector has to match the selector identifier or be empty";

        public UpsertEndpointDtoValidator(string upsertSelector)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            Include(new EndpointDtoValidator());
            RuleFor(x => x.Selector)
                .Empty().When(x => string.IsNullOrWhiteSpace(x.Selector))
                .WithMessage(SelectorValidationMessage)
                .Equal(upsertSelector).When(x => !string.IsNullOrWhiteSpace(x.Selector))
                .WithMessage(SelectorValidationMessage);
        }
    }
}