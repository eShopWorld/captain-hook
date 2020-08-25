using CaptainHook.Contract;
using FluentValidation;

namespace CaptainHook.Application.Validators.Dtos
{
    public class UpsertSubscriberEndpointDtoValidator : AbstractValidator<EndpointDto>
    {
        public UpsertSubscriberEndpointDtoValidator()
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            Include(new EndpointDtoValidator());
            RuleFor(x => x.Selector).NotEmpty();
        }
    }
}