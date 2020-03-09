using Microsoft.Azure.Documents;

namespace CaptainHook.Database.CosmosDB
{
    public class QueryDefinition
    {
        public readonly SqlQuerySpec QuerySpec;
        public readonly bool RequiresCrossPartitions;

        public QueryDefinition (SqlQuerySpec querySql, bool requiresCrossPartitions)
        {
            QuerySpec = querySql;
            RequiresCrossPartitions = requiresCrossPartitions;
        }

        public QueryDefinition (string sql, bool requiresCrossPartitions)
        {
            QuerySpec = new SqlQuerySpec(sql);
            RequiresCrossPartitions = requiresCrossPartitions;
        }
    }
}