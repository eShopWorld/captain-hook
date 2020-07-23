using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Sdk;

namespace CaptainHook.TestsInfrastructure
{
    public class GetInvalidEnumValuesAttribute : DataAttribute
    {
        private readonly object[] _validEnumValues;
        private readonly Type _enumType;

        public GetInvalidEnumValuesAttribute(params object[] validEnumValues)
        {
            if (validEnumValues == null || validEnumValues?.Length == 0)
                throw new ArgumentException("At least one valid enum value must be provided", nameof(validEnumValues));

            var firstItemType = validEnumValues.First().GetType();

            if (!firstItemType.IsEnum)
                throw new ArgumentException("Passed argument must be an enum value", nameof(_validEnumValues));

            if (validEnumValues.All(v => v.GetType() != firstItemType))
                throw new ArgumentException("All provided values must be of same type", nameof(validEnumValues));

            _validEnumValues = validEnumValues;
            _enumType = firstItemType;
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            if (testMethod == null) { throw new ArgumentNullException(nameof(testMethod)); }

            var allEnumValues = Enum.GetValues(_enumType);
            foreach (var value in allEnumValues)
            {
                foreach (var validEnumValue in _validEnumValues)
                {
                    if (!value.Equals(validEnumValue))
                    {
                        yield return new[] { value };
                    }
                }
            }
        }
    }
}