using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FluentValidation;

namespace CaptainHook.Domain.Entities
{
    public class UriTransformValidator : AbstractValidator<UriTransformEntity>
    {
        private readonly string[] _uris;
        private static readonly Regex _extractSelectorsFromUri = new Regex("(?<=\\{).+?(?=\\})", RegexOptions.Compiled);

        public UriTransformValidator(IEnumerable<EndpointEntity> endpoints)
        {
            _uris = endpoints.Select(x => x.Uri).ToArray();

            RuleFor(x => x.Replace).NotEmpty()
                .Must(ContainAllReplacementsForUris).WithMessage("URI Transform dictionary must contain all the placeholders defined in each URI");
        }

        private bool ContainAllReplacementsForUris(IDictionary<string, string> replace)
        {
            var values = _uris
                .SelectMany(u => _extractSelectorsFromUri.Matches(u))
                .Select(m => m.Value)
                .Where(DoesNotContainSelectorString);
            return values.All(replace.ContainsKey);
        }

        private bool DoesNotContainSelectorString(string value)
        {
            return !value.Equals("selector", System.StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
