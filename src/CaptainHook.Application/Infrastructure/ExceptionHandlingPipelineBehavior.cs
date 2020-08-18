using System;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Domain.Errors;
using CaptainHook.Domain.Results;
using Eshopworld.Core;
using MediatR;

namespace CaptainHook.Application.Infrastructure
{
    public class ExceptionHandlingPipelineBehavior<TRequest, TData> : IPipelineBehavior<TRequest, OperationResult<TData>>
    {
        private readonly IBigBrother _bigBrother;

        public ExceptionHandlingPipelineBehavior(IBigBrother bigBrother)
        {
            _bigBrother = bigBrother;
        }

        public async Task<OperationResult<TData>> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<OperationResult<TData>> next)
        {
            try
            {
                return await next();
            }
            catch (Exception ex)
            {
                _bigBrother.Publish(ex.ToExceptionEvent());
                var error = new UnhandledExceptionError($"Error processing {typeof(TRequest).Name}", ex);
                return new OperationResult<TData>(error);
            }
        }
    }
}