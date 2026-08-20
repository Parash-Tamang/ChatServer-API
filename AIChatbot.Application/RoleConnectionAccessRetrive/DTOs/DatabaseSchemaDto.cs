namespace AIChatbot.Application.RoleConnectionAccessRetrive.DTOs;

public class DatabaseSchemaDto
{
    public string SchemaName { get; set; } = default!;

    public string TableName { get; set; } = default!;

    public List<string> Columns { get; set; } = [];
}