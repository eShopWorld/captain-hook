using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            var validationResults = _validators.Select(validator => validator.Validate(request)).ToArray();
            var validationError = new ValidationErrorBuilder().Build(validationResults);

            if (validationError != null)
            {
                return Task.FromResult((TResponse)Activator.CreateInstance(typeof(TResponse), validationError));
            }

            return next();
        }
    }
}