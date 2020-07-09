using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Domain.Common;
using FluentValidation;
using MediatR;

namespace CaptainHook.Domain.Handlers.Subscribers
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
                var failures = validationFailures.Select(x => new Failure { Code = x.ErrorCode, Message = x.ErrorMessage, Property = x.PropertyName}).ToList();
                var validationError = new ValidationError("Invalid request", failures);
                return Task.FromResult((TResponse)Activator.CreateInstance(typeof(TResponse), validationError));
            }

            return next();
        }
    }
}