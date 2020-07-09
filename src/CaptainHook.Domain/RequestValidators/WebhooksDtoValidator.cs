//using CaptainHook.Contract;
//using FluentValidation;

//namespace CaptainHook.Api.Controllers
//{
//    public class WebhooksDtoValidator : AbstractValidator<WebhooksDto>
//    {
//        public WebhooksDtoValidator()
//        {
//            RuleFor(x => x.SelectionRule).MinimumLength(10).Must(p => p.StartsWith("sel"));
//            RuleForEach(x => x.Endpoints).NotEmpty().SetValidator(new EndpointValidator());
//        }
//    }
//}