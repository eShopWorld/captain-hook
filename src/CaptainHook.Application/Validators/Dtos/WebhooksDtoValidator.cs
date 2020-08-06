using CaptainHook.Contract;
using FluentValidation;
using Newtonsoft.Json.Linq;

namespace CaptainHook.Application.Validators.Dtos
{
    public class WebhooksDtoValidator : AbstractValidator<WebhooksDto>
    {
        private static JObject _jObject = new JObject();

        public WebhooksDtoValidator()
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(x => x.Endpoints).NotEmpty()
                .Must(list => list.Count >= 1).WithMessage("Webhooks list must contain at list one endpoint");

            RuleForEach(x => x.Endpoints)
                .SetValidator(new EndpointDtoValidator());

            RuleFor(x => x.SelectionRule).NotEmpty()
                .Must(BeValidJsonPathExpression);
        }

        private bool BeValidJsonPathExpression(string selectionRule)
        {
            if(!selectionRule.StartsWith('$'))
            {
                return false;
            }

            try
            {
                _jObject.SelectToken(selectionRule, false);
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}