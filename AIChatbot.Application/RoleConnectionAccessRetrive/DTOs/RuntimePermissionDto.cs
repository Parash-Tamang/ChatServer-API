namespace AIChatbot.Application.RoleConnectionAccessRetrive.DTOs;

public class RuntimePermissionDto
{
    public string SchemaName { get; set; } = default!;

    public string TableName { get; set; } = default!;

    public string ColumnName { get; set; } = default!;

    public string FilterType { get; set; } = default!;

    public string? RuntimeKey { get; set; }

    public object? Values { get; set; }
}