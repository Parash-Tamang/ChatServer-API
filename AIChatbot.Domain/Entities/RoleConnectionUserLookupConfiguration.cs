public class RoleConnectionUserLookupConfiguration
{
    public Guid Id { get; set; }

    public string RoleId { get; set; }

    public Guid ConnectionId { get; set; }

    public string UserTableName { get; set; }

    public string UserIdColumn { get; set; }

    public string? EmailColumn { get; set; }

    public string? PhoneColumn { get; set; }

    public DateTime CreatedAt { get; set; }
}