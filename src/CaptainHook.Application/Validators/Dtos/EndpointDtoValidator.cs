using CaptainHook.Application.Validators.Common;
using CaptainHook.Contract;
using FluentValidation;

namespace CaptainHook.Application.Validators.Dtos
{
    public class EndpointDtoValidator : AbstractValidator<EndpointDto>
    {
        public EndpointDtoValidator()
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(x => x.HttpVerb).NotEmpty()
                .SetValidator(new HttpVerbValidator());
            RuleFor(x => x.Uri).NotEmpty()
                .SetValidator(new UriValidator());
            RuleFor(x => x.Authentication).NotNull()
                .SetValidator(new AuthenticationDtoValidator());
            RuleFor(x => x.UriTransform)
                .SetValidator(new UriTransformValidator());
        }
    }
}