using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace CaptainHook.TestsInfrastructure.Builders
{
    public class SimpleBuilder<T>
    {
        private readonly List<IPropertySetterCall<T>> _propertySetterCalls = new List<IPropertySetterCall<T>>();

        public SimpleBuilder<T> With<TProperty>(Expression<Func<T, TProperty>> property, TProperty value)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            if (!((property.Body as MemberExpression)?.Member is PropertyInfo propertyInfo))
                throw new ArgumentException("Expression is not a property expression", nameof(property));

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