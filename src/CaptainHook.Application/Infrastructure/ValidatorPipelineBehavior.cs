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
    public class ValidatorPipelineBehavior<TRequest, TData> : IPipelineBehavior<TRequest, OperationResult<TData>>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidatorPipelineBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<OperationResult<TData>> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<OperationResult<TData>> next)
        {
            var validationResults = _validators.Select(validator => validator.Validate(request)).ToArray();
            var validationError = new ValidationErrorBuilder().Build(validationResults);

            if (validationError != null)
            {
                return new OperationResult<TData>(validationError);
            }

            return await next();
        }
    }
}