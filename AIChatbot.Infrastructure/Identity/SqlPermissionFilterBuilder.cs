using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Common;
using AIChatbot.Domain.Entities;
using System.Text.Json;

namespace AIChatbot.Infrastructure.Identity;

public class SqlPermissionFilterBuilder
    : ISqlPermissionFilterBuilder
{
    public SqlFilterResult Build(
        List<RoleRuntimePermission> permissions,

        RuntimeContextResult context,

        string schemaName,

        string tableName)
    {
        var result = new SqlFilterResult();

        var filters =
            permissions
                .Where(x =>
                    x.SchemaName == schemaName &&
                    x.TableName == tableName)
                .ToList();

        var whereParts =
            new List<string>();

        int index = 0;

        foreach (var filter in filters)
        {
            var paramName =
                $"p{index++}";

            // ========================================================
            // RUNTIME FILTER
            // ========================================================

            if (filter.FilterType == "runtime_id")
            {
                if (filter.RuntimeKey == null)
                    continue;

                if (!context.Values.TryGetValue(
                        filter.RuntimeKey,
                        out var runtimeValues))
                {
                    continue;
                }

                if (runtimeValues.Count == 0)
                {
                    continue;
                }

                whereParts.Add(
                    $"[{filter.ColumnName}] IN @{paramName}");

                result.Parameters[paramName] =
                    runtimeValues;
            }

            // ========================================================
            // ENUM / BOOL / FIXED
            // ========================================================

            else
            {
                if (string.IsNullOrWhiteSpace(
                        filter.FilterValuesJson))
                {
                    continue;
                }

                var values =
                    JsonSerializer.Deserialize<
                        List<string>>(
                        filter.FilterValuesJson);

                if (values == null ||
                    values.Count == 0)
                {
                    continue;
                }

                whereParts.Add(
                    $"[{filter.ColumnName}] IN @{paramName}");

                result.Parameters[paramName] =
                    values;
            }
        }

        result.FilterSql =
            string.Join(" AND ", whereParts);

        return result;
    }
}