using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using CaptainHook.Application.Requests.Subscribers;
using CaptainHook.Application.RequestValidators;
using CaptainHook.Contract;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Xunit;

namespace CaptainHook.Application.Tests.RequestValidators
{
    public class UpsertSubscriberRequestValidatorTests
    {
        [Fact, IsUnit]
        public void For_invalid_request_should_call_validator_and_return_Error()
        {
            var dto = new SimpleBuilder<SubscriberDto>()
                .With(x => x.Name, "subscriber name")
                .With(x => x.Webhooks, new SimpleBuilder<WebhooksDto>()
                    .With(w => w.SelectionRule, "new rule")
                    .Create())
                .Create();
            var request = new UpsertSubscriberRequest("event", "subscriber", dto);

            var result = new UpsertSubscriberRequestValidator().Validate(request);

            result.IsValid.Should().BeTrue();
        }
    }

    public class SimpleBuilder<T>
    {
        private readonly List<IPropertySetterCall<T>> _propertySetterCalls = new List<IPropertySetterCall<T>>();

        public SimpleBuilder<T> With<TProperty>(Expression<Func<T, TProperty>> property, TProperty value)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));
            if (!(property.Body is MemberExpression))
                throw new ArgumentException("Expression is not property expression", nameof(property));
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var call = new PropertySetterCall<TProperty>(property, value);
            _propertySetterCalls.Add(call);

            return this;
        }

        public T Create()
        {
            var result = Activator.CreateInstance<T>();
            foreach (var call in _propertySetterCalls)
            {
                call.SetValue(result);
            }
            return result;
        }

        private interface IPropertySetterCall<T>
        {
            void SetValue(T instance);
        }

        private class PropertySetterCall<TProperty> : IPropertySetterCall<T>
        {
            public Expression<Func<T, TProperty>> Property { get; }
            public TProperty Value { get; }

            public PropertySetterCall(Expression<Func<T, TProperty>> property, TProperty value)
            {
                Property = property;
                Value = value;
            }

            public void SetValue(T instance)
            {
                var property = (Property.Body as MemberExpression)?.Member as PropertyInfo;
                property?.SetValue(instance, Value, null);
            }
        }
    }
}
