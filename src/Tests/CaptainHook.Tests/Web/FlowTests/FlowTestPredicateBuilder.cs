using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using NuGet.Frameworks;

namespace CaptainHook.Tests.Web.FlowTests
{
    public class FlowTestPredicateBuilder
    {
        private List<Func<ProcessedEventModel, bool>> _subPredicates = new List<Func<ProcessedEventModel, bool>>();
        private bool _callbackMode;

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
                _callbackMode==m.IsCallback ^ endsWithId ); //XOR

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

        /// <summary>
        /// check that the tracked request was callback
        /// </summary>
        /// <returns>predicate builder</returns>
        public FlowTestPredicateBuilder CheckIsCallback(bool expectStatusCode=true, string statusCodeName ="StatusCode", bool expectContent=true, string httpContentName = "Content")
        {
            _callbackMode = true;
            _subPredicates.Add(m =>
                {
                    var payload = JObject.Parse(m.Payload);

                    var statusCode = expectStatusCode? payload[statusCodeName]: new JObject();
                    var content = expectContent? payload[httpContentName] : new JObject();

                    return m.IsCallback &&
                           statusCode != null &&
                           content != null;
                }
            );

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
        /// build overall check delegate (used in fluent assertions) - check "all" match the predicate
        /// </summary>
        /// <returns>test delegate</returns>
        public Func<ProcessedEventModel, bool> BuildMatchesAll() => model =>
        {
            return _subPredicates.All(i => i.Invoke(model));
        };

        public bool AllSubPredicatesMatch(ProcessedEventModel model)
        {
            return BuildMatchesAll().Invoke(model);
        } 

        /// <summary>
        /// build overall check delegate (used in fluent assertions), check "any" exists
        /// </summary>
        /// <returns>test delegate</returns>
        public Func<ProcessedEventModel, bool> BuildMatchesAny() => model =>
        {
            return _subPredicates.Exists(i => i.Invoke(model));
        };

        public bool AnySubPredicateMatches(ProcessedEventModel model)
        {
            return BuildMatchesAny().Invoke(model);
        }
    }
}
