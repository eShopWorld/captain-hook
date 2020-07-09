using CaptainHook.Contract;
using FluentValidation;

namespace CaptainHook.Api.Controllers
{
    public class EndpointValidator : AbstractValidator<EndpointDto>
    {
        public EndpointValidator()
        {
            RuleFor(x => x.Selector).NotEmpty();
        }
    }
}