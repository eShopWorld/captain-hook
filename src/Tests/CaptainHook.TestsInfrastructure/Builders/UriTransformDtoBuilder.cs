using CaptainHook.Contract;
using System.Collections.Generic;

namespace CaptainHook.TestsInfrastructure.Builders
{
    public class UriTransformDtoBuilder : SimpleBuilder<UriTransformDto>
    {
        public UriTransformDtoBuilder()
        {
            With(e => e.Replace, new Dictionary<string, string>());
        }
    }
}
