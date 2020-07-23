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
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (!((property.Body as MemberExpression)?.Member is PropertyInfo propertyInfo))
                throw new ArgumentException("Expression is not property expression", nameof(property));

            _propertySetterCalls.Add(new PropertySetterCall<TProperty>(propertyInfo, value));
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

        private interface IPropertySetterCall<in TType>
        {
            void SetValue(TType instance);
        }

        private class PropertySetterCall<TProperty> : IPropertySetterCall<T>
        {
            private readonly PropertyInfo _property;
            private readonly TProperty _value;

            public PropertySetterCall(PropertyInfo property, TProperty value)
            {
                _property = property;
                _value = value;
            }

            public void SetValue(T instance)
            {
                _property.SetValue(instance, _value, null);
            }
        }
    }
}
