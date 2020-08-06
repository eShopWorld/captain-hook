using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CaptainHook.Contract;
using FluentValidation;

namespace CaptainHook.Application.Validators.Common
{
    public class UriTransformValidator : AbstractValidator<UriTransformDto>
    {
        private readonly string _uri;
        private static readonly Regex _extractSelectorsFromUri = new Regex("(?<=\\{).+?(?=\\})", RegexOptions.Compiled);

        public UriTransformValidator(string uri)
        {
            _uri = uri;

            CascadeMode = CascadeMode.StopOnFirstFailure;

            // selector in Replace dictionary is required only temporary
            RuleFor(x => x.Replace).NotEmpty()
                .Must(x => x.ContainsKey("selector")).WithMessage("Routes dictionary must temporarily contain an item with 'selector' key")
                .Must(ContainAllReplacementsForUri).WithMessage("Routes dictionary must contain all items defined in the Uri");
        }

        private bool ContainAllReplacementsForUri(IDictionary<string, string> replace)
        {
            var values = _extractSelectorsFromUri.Matches(_uri).Select(m => m.Value);
            return values.All(replace.ContainsKey);
        }
    }
}
