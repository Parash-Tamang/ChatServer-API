namespace AIChatbot.Application.Common;

public class ExternalUserLookupResult
{
    public bool Found { get; set; }

    public string TableName { get; set; } = default!;

    public string UserId { get; set; } = default!;

    public string MatchColumn { get; set; } = default!;

    public string MatchValue { get; set; } = default!;
}