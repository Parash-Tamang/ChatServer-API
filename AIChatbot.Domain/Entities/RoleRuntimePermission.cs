using Microsoft.AspNetCore.Identity;

namespace AIChatbot.Domain.Entities;

public class RoleRuntimePermission
{
    public Guid Id { get; set; }

    // ============================================================
    // ROLE + DB
    // ============================================================

    public string RoleId { get; set; } = default!;

    public Guid ConnectionId { get; set; }

    // ============================================================
    // TARGET
    // ============================================================

    public string SchemaName { get; set; } = default!;

    public string TableName { get; set; } = default!;

    public string ColumnName { get; set; } = default!;

    // ============================================================
    // FILTER CONFIG
    // ============================================================

    // runtime_id
    // enum
    // bool
    // fixed
    // range
    public string FilterType { get; set; } = default!;

    // runtime context key
    // Example:
    // CustomerID
    // TerritoryID
    public string? RuntimeKey { get; set; }

    // JSON
    // Example:
    // [1,2,3]
    // ["Pending"]
    // [true]
    public string? FilterValuesJson { get; set; }

    public DateTime CreatedAt { get; set; }

    // ============================================================
    // NAVIGATION
    // ============================================================
    public string AccessLevel { get; set; }
    = string.Empty;

    public string? Reason { get; set; }
    public IdentityRole Role { get; set; } = default!;

    public ConnectionString Connection { get; set; } = default!;
}