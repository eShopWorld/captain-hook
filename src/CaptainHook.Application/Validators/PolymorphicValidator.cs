using FluentValidation;
using FluentValidation.Results;
using FluentValidation.Validators;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CaptainHook.Application.Validators
{
	public class PolymorphicValidator<T, TInterface> : ChildValidatorAdaptor<T, TInterface>
	{
		readonly Dictionary<Type, IValidator> _derivedValidators = new Dictionary<Type, IValidator>();

		// Need the base constructor call, even though we're just passing null.
		public PolymorphicValidator() : base((IValidator<TInterface>)null, typeof(IValidator<TInterface>))
		{
		}

		public PolymorphicValidator<T, TInterface> Add<TDerived>(IValidator<TDerived> derivedValidator) where TDerived : TInterface
		{
			_derivedValidators[typeof(TDerived)] = derivedValidator;
			return this;
		}

		public override IValidator<TInterface> GetValidator(PropertyValidatorContext context)
		{
			// bail out if the current item is null
			if (context.PropertyValue == null) return null;

			if (_derivedValidators.TryGetValue(context.PropertyValue.GetType(), out var derivedValidator))
			{
				return new ValidatorWrapper(derivedValidator);
			}

			return null;
		}

		private class ValidatorWrapper : AbstractValidator<TInterface>
		{

			private IValidator _innerValidator;
			public ValidatorWrapper(IValidator innerValidator)
			{
				_innerValidator = innerValidator;
			}

			public override ValidationResult Validate(ValidationContext<TInterface> context)
			{
				return _innerValidator.Validate(context);
			}

			public override Task<ValidationResult> ValidateAsync(ValidationContext<TInterface> context, CancellationToken cancellation = new CancellationToken())
			{
				return _innerValidator.ValidateAsync(context, cancellation);
			}

			public override IValidatorDescriptor CreateDescriptor()
			{
				return _innerValidator.CreateDescriptor();
			}
		}
	}
}
