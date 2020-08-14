using System;
using Eshopworld.Tests.Core;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using CaptainHook.Application.Infrastructure;
using CaptainHook.Domain.Errors;
using CaptainHook.Domain.Results;
using Eshopworld.Core;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Moq;
using Xunit;

namespace CaptainHook.Application.Tests
{
    public class MediatorPipelineBehaviorsTests
    {
        private readonly IMediator _mediator;

        public MediatorPipelineBehaviorsTests()
        {
            var thisAssembly = Assembly.GetExecutingAssembly();

            var builder = new ContainerBuilder();
            builder.RegisterInstance(Mock.Of<IBigBrother>());
            builder.RegisterMediatorInfrastructure(thisAssembly, thisAssembly);
            var container = builder.Build();

            _mediator = container.Resolve<IMediator>();
        }

        [Fact, IsUnit]
        public async void When_RequestIsInvalid_Then_ValidatorIsCalledAndErrorReturned()
        {
            var request = new DivideRequest { Dividend = 6, Divisor = 0 };

            var result = await _mediator.Send(request);

            result.Should().BeOfType<OperationResult<DivideResult>>().Which.Error.Should().BeOfType<ValidationError>();
        }

        [Fact, IsUnit]
        public async void When_RequestIsValid_Then_ValidatorIsCalledAndDataReturned()
        {
            var request = new DivideRequest { Dividend = 6, Divisor = 2 };

            var result = await _mediator.Send(request);

            OperationResult<DivideResult> expectedResult = new DivideResult { Quotient = 3 };
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Fact, IsUnit]
        public async void When_HandlerThrowsException_Then_UnhandledExceptionErrorIsReturned()
        {
            var request = new ThrowExceptionRequest();

            var result = await _mediator.Send(request);

            OperationResult<bool> expectedResult = new UnhandledExceptionError($"Error processing {nameof(ThrowExceptionRequest)}", new Exception());
            result.Should().BeEquivalentTo(expectedResult);
        }


        public class DivideRequest : IRequest<OperationResult<DivideResult>>
        {
            public double Dividend { get; set; }
            public double Divisor { get; set; }
        }

        public class DivideResult
        {
            public double Quotient { get; set; }
        }

        private class DivideRequestHandler : IRequestHandler<DivideRequest, OperationResult<DivideResult>>
        {
            public async Task<OperationResult<DivideResult>> Handle(DivideRequest request, CancellationToken cancellationToken)
            {
                await Task.CompletedTask;
                return new DivideResult { Quotient = request.Dividend / request.Divisor };
            }
        }

        public class DivideRequestValidator : AbstractValidator<DivideRequest>
        {
            public DivideRequestValidator()
            {
                RuleFor(r => r.Divisor).NotEqual(0);
            }
        }

        public class ThrowExceptionRequest : IRequest<OperationResult<bool>>
        {
        }

        private class ThrowExceptionRequestHandler : IRequestHandler<ThrowExceptionRequest, OperationResult<bool>>
        {
            public Task<OperationResult<bool>> Handle(ThrowExceptionRequest request, CancellationToken cancellationToken)
            {
                throw new Exception();
            }
        }
    }
}
