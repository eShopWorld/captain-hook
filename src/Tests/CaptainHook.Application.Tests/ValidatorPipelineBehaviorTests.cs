using Eshopworld.Tests.Core;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using CaptainHook.Application.Infrastructure;
using CaptainHook.Domain.Results;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Xunit;

namespace CaptainHook.Application.Tests
{
    public class ValidatorPipelineBehaviorTests
    {
        private readonly IMediator _mediator;

        public ValidatorPipelineBehaviorTests()
        {
            var container = new ContainerBuilder()
                .RegisterMediatorInfrastructure(Assembly.GetExecutingAssembly())
                .Build();

            _mediator = container.Resolve<IMediator>();
        }

        [Fact, IsUnit]
        public async void For_invalid_request_should_call_validator_and_return_Error()
        {
            var request = new DivideRequest { Dividend = 6, Divisor = 0 };

            var result = await _mediator.Send(request);

            result.Should().BeOfType<OperationResult<DivideResult>>().Which.IsError.Should().BeTrue();
        }

        [Fact, IsUnit]
        public async void For_valid_request_should_call_validator_and_return_data()
        {
            var request = new DivideRequest { Dividend = 6, Divisor = 2 };

            var result = await _mediator.Send(request);

            result.Should().BeOfType<OperationResult<DivideResult>>().Which.IsError.Should().BeFalse();
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
    }
}
