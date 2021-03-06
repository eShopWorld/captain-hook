﻿using System;
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

            // selector in Replace dictionary is required only temporary
            RuleFor(x => x.Replace).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(ContainAllReplacementsForUris).WithMessage("URI Transform dictionary must contain all the placeholders defined in each URI");
        }

        private bool ContainAllReplacementsForUris(IDictionary<string, string> replace)
        {
            var values = _uris
                .SelectMany(u => _extractSelectorsFromUri.Matches(u))
                .Select(m => m.Value)
                .Where(DoesNotContainSelectorString);
            return !values.Except(replace.Keys, StringComparer.OrdinalIgnoreCase).Any();
        }

        private bool DoesNotContainSelectorString(string value)
        {
            return !value.Equals("selector", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
