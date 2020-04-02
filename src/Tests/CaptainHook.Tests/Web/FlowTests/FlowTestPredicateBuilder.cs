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


        /// <summary>
        /// check specific verb seen for the tracked event
        /// </summary>
        /// <param name="expectedVerb">the verb expected</param>
        /// <returns>builder chain instance</returns>
        public FlowTestPredicateBuilder CheckVerb(HttpMethod expectedVerb)
        {
            _subPredicates.Add(m=>
                m.Verb.Equals(expectedVerb.Method, StringComparison.OrdinalIgnoreCase));

            return this;
        }

        /// <summary>
        /// check whether URL was suffixed with id (as extracted from payload based on config) or not
        /// </summary>
        /// <param name="endsWithId">flag to drive the positive or negative check</param>
        /// <returns>builder chain instance</returns>
        public FlowTestPredicateBuilder CheckUrlIdSuffixPresent(bool endsWithId)
        {
            _subPredicates.Add(m=> 
                m.Url.EndsWith("/intake", StringComparison.OrdinalIgnoreCase) ^ endsWithId ); //XOR

            return this;
        }

        /// <summary>
        /// check OIDC scope being present in the tracked event
        /// </summary>
        /// <param name="requiredScopes">list of scopes required</param>
        /// <returns>builder chain instance</returns>
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

        /// <summary>
        /// build overall check delegate (used in fluent assertions)
        /// </summary>
        /// <returns>test delegate</returns>
        public Func<ProcessedEventModel, bool> Build() => model =>
        {
            return _subPredicates.All(i => i.Invoke(model));
        };
    }
}
