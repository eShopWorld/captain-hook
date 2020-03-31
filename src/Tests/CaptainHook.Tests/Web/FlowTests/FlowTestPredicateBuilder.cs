using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;

namespace CaptainHook.Tests.Web.FlowTests
{
    public class FlowTestPredicateBuilder
    {
        private List<Func<ProcessedEventModel, bool>> _subPredicates = new List<Func<ProcessedEventModel, bool>>();

        public FlowTestPredicateBuilder Reset()
        {
            _subPredicates = new List<Func<ProcessedEventModel, bool>>();

            return this;
        }

        public FlowTestPredicateBuilder CheckVerb(HttpMethod expectedVerb)
        {
            _subPredicates.Add(m=>
                m.Verb.Equals(expectedVerb.Method, StringComparison.OrdinalIgnoreCase));

            return this;
        }

        public FlowTestPredicateBuilder CheckUrl(bool endsWithId = false)
        {
            _subPredicates.Add(m=> 
                m.Url.EndsWith("/intake", StringComparison.OrdinalIgnoreCase) ^ endsWithId ); //XOR

            return this;
        }

        public FlowTestPredicateBuilder CheckOidcAuthScopes(params string[] requiredScopes)
        {
            _subPredicates.Add(m =>
            {
                var jwt = ParseJwt(m);

                return requiredScopes.All(s => jwt.Claims.FirstOrDefault(c =>
                    c.Type.Equals("scope", StringComparison.OrdinalIgnoreCase) &&
                    c.Value.Equals(s, StringComparison.Ordinal))!=null);
            });

            return this;
        }

        private static JwtSecurityToken ParseJwt(ProcessedEventModel m)
        {
            //check this is "bearer" token
            if (string.IsNullOrWhiteSpace(m.Authorization) ||
                !m.Authorization.StartsWith("bearer", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("This is not expected OIDC authorization header", nameof(m.Authorization));

            var tokenItself = m.Authorization.Substring("bearer ".Length); //space important here

            var tokenDecoder = new JwtSecurityTokenHandler();
            return (JwtSecurityToken)tokenDecoder.ReadToken(tokenItself);
        }

        public Func<ProcessedEventModel, bool> Build() => model =>
        {
            return _subPredicates.All(i => i.Invoke(model));
        };
    }
}
