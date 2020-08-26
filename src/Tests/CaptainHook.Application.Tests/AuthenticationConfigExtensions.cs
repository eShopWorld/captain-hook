using CaptainHook.Common.Authentication;

namespace CaptainHook.Application.Tests
{
    public static class AuthenticationConfigExtensions
    {
        public static AuthenticationConfigAssertions Should(this AuthenticationConfig instance)
        {
            return new AuthenticationConfigAssertions(instance);
        }
    }
}
