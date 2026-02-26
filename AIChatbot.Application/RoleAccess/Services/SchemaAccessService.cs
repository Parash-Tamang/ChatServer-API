using AIChatbot.Application.Abstractions;

namespace AIChatbot.Application.RoleAccess.Services;

public class SchemaAccessService
{
    private readonly ISchemaRepository _schemaRepo;

    public SchemaAccessService(ISchemaRepository schemaRepo)
    {
        _schemaRepo = schemaRepo;
    }

    public async Task<Dictionary<string, List<string>>> GetFilteredSchemaAsync(
        Guid dbId,
        Dictionary<string, List<string>> roleTableColumns)
    {
        var fullSchema = await _schemaRepo.GetSchemaAsync(dbId);

        var filtered = new Dictionary<string, List<string>>();

        foreach (var table in roleTableColumns.Keys)
        {
            if (!fullSchema.ContainsKey(table))
                continue;

            var allowedColumns = roleTableColumns[table];

            // if no column restriction → allow full table schema
            filtered[table] = allowedColumns.Count == 0
                ? fullSchema[table]
                : fullSchema[table].Where(c => allowedColumns.Contains(c)).ToList();
        }

        return filtered;
    }
}