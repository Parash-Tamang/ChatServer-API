public sealed class ChatMessageResult
{
    public Guid Id { get; init; }

    public string Role { get; init; } = default!;

    public string Content { get; init; } = default!;

    public DateTime CreatedAt { get; init; }

    public bool ExcelGenerated { get; set; }

    public object? ExcelAvailableNow { get; set; }

    public string? GraphType { get; set; }

    public string? GraphTitle { get; set; }

    public string? GraphImageUrl { get; set; }
    public string? GraphImageBase64 { get; set; }

    public bool HasGraph { get; set; }
}