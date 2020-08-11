﻿using System.Linq;
using CaptainHook.Domain.Results;
using FluentValidation.Results;

namespace CaptainHook.Domain.Errors
{
    public class ValidationErrorBuilder
    {
        public ValidationError Build(params ValidationResult[] validationResults)
        {
            var validationFailures = (validationResults ?? Enumerable.Empty<ValidationResult>())
                .SelectMany(result => result.Errors)
                .Where(error => error != null)
                .ToList();

            if (! validationFailures.Any())
            {
                return null;
            }

            var failures = validationFailures.Select(x => new Failure(x.ErrorCode, x.ErrorMessage, x.PropertyName));
            var validationError = new ValidationError("Invalid request", failures.ToArray());

            return validationError;
        }
    }
}