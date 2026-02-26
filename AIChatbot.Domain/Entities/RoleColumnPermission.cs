namespace AIChatbot.Domain.Entities;

public class RoleColumnPermission
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ConnectionStringId { get; set; }
    public ConnectionString ConnectionString { get; set; } = default!;

    public string RoleName { get; set; } = default!;
    public string TableName { get; set; } = default!;
    public string ColumnName { get; set; } = default!;
}