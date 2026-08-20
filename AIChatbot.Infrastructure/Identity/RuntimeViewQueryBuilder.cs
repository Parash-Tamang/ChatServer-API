using AIChatbot.Application.Abstractions;
using AIChatbot.Domain.Entities;

namespace AIChatbot.Infrastructure.Identity;

public class RuntimeViewQueryBuilder
    : IRuntimeViewQueryBuilder
{
    public string Build(
        string viewName,

        List<RoleRuntimePermission> permissions)
    {
        var runtimePermissions =
            permissions
                .Where(x =>
                    x.FilterType == "runtime_id")
                .ToList();

        if (runtimePermissions.Count == 0)
        {
            return $@"
CREATE VIEW [{viewName}]
AS
SELECT 1 AS RuntimeValue
";
        }

        // ============================================================
        // BASE TABLE
        // ============================================================

        var first =
            runtimePermissions.First();

        var baseAlias = "t1";

        var selectParts =
            new List<string>();

        var joinParts =
            new List<string>();

        var usedTables =
            new Dictionary<string, string>();

        usedTables[
            $"{first.SchemaName}.{first.TableName}"
        ] = baseAlias;

        selectParts.Add(
            $"{baseAlias}.[{first.ColumnName}] AS [{first.ColumnName}]");

        var aliasCounter = 2;

        // ============================================================
        // BUILD JOINS
        // ============================================================

        foreach (var permission
                 in runtimePermissions.Skip(1))
        {
            var tableKey =
                $"{permission.SchemaName}.{permission.TableName}";

            string alias;

            if (!usedTables.TryGetValue(
                    tableKey,
                    out alias!))
            {
                alias = $"t{aliasCounter++}";

                usedTables[tableKey] = alias;

                // ====================================================
                // SIMPLE COLUMN MATCH JOIN
                // ====================================================

                joinParts.Add($@"
LEFT JOIN
    [{permission.SchemaName}].[{permission.TableName}] {alias}
ON
    {baseAlias}.[{first.ColumnName}]
    =
    {alias}.[{first.ColumnName}]
");
            }

            selectParts.Add(
                $"{alias}.[{permission.ColumnName}] AS [{permission.ColumnName}]");
        }

        // ============================================================
        // FINAL SQL
        // ============================================================

        var sql = $@"
CREATE VIEW [{viewName}]
AS
SELECT
    {string.Join(",\n    ", selectParts)}
FROM
    [{first.SchemaName}].[{first.TableName}] {baseAlias}

{string.Join("\n", joinParts)}
";

        return sql;
    }
}