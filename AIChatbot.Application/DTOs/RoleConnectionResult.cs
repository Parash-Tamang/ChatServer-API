public sealed class RoleConnectionResult
{
    public string RoleId { get; set; } = default!;

    public string RoleName { get; set; } = default!;

    public Guid ConnectionId { get; set; }

    public string ServerName { get; set; } = default!;

    public string DatabaseName { get; set; } = default!;

    public string AuthMode { get; set; } = default!;
}