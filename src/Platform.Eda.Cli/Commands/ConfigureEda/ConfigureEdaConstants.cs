using System.Linq;

namespace Platform.Eda.Cli.Commands.ConfigureEda
{
    public class ConfigureEdaConstants
    {
        public class EnvironmentNames
        {
            public static bool Contains(string env)
            {
                return ValidEnvironmentNames.Contains(env);
            }

            public const string Ci = "ci";
            public const string Test = "test";
            public const string Prep = "prep";
            public const string Sand = "sand";
            public const string Prod = "prod";

            private static readonly string[] ValidEnvironmentNames = {
                Ci,
                Test,
                Prep,
                Sand,
                Prod
            };
        }
    }
}
