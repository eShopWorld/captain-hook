using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CaptainHook.Contract;
using FluentValidation;

namespace CaptainHook.Application.Validators.Common
{
    public class UriTransformValidator : AbstractValidator<UriTransformDto>
    {
        private readonly string[] _uris;
        private static readonly Regex _extractSelectorsFromUri = new Regex("(?<=\\{).+?(?=\\})", RegexOptions.Compiled);

        public UriTransformValidator(IEnumerable<EndpointDto> endpoints)
        {
            _uris = endpoints.Select(x => x.Uri).ToArray();

            CascadeMode = CascadeMode.StopOnFirstFailure;

            // selector in Replace dictionary is required only temporary
            RuleFor(x => x.Replace).NotEmpty()
                .Must(ContainAllReplacementsForUris).WithMessage("Routes dictionary must contain all the placeholders defined in each URI");
        }

        private bool ContainAllReplacementsForUris(IDictionary<string, string> replace)
        {
            var values = _uris.SelectMany(u => _extractSelectorsFromUri.Matches(u)).Select(m => m.Value);
            return values.All(replace.ContainsKey);
        }
    }
}
