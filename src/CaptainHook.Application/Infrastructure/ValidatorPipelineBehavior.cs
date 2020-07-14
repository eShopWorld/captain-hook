using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Domain.Errors;
using CaptainHook.Domain.Results;
using FluentValidation;
using MediatR;

namespace CaptainHook.Application.Infrastructure
{
    public class ValidatorPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidatorPipelineBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var validationResults = _validators.Select(validator => validator.Validate(request));

            var validationFailures = validationResults
                .SelectMany(result => result.Errors)
                .Where(error => error != null)
                .ToList();

            if (validationFailures.Any())
            {
                var failures = validationFailures.Select(x => new Failure(x.ErrorCode, x.ErrorMessage, x.PropertyName));
                var validationError = new ValidationError("Invalid request", failures.ToArray());
                return Task.FromResult((TResponse)Activator.CreateInstance(typeof(TResponse), validationError));
            }

            return next();
        }
    }
}