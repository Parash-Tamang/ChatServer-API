using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Chat.Queries;
using AIChatbot.Application.Chat.Results;
using MediatR;
using System.Text.Json;

namespace AIChatbot.Application.Chat.Handlers;

/// Retrieves all messages for a chat session
public class GetChatMessagesHandler
    : IRequestHandler<GetChatMessagesQuery, IReadOnlyList<ChatMessageResult>>
{
    private readonly IChatSessionRepository _repo;
    private readonly IResponseMetadataRepository _metadataRepo;

    public GetChatMessagesHandler(IChatSessionRepository repo, IResponseMetadataRepository metadataRepo)
    {
        _repo = repo;
        _metadataRepo = metadataRepo;
    }

    public async Task<IReadOnlyList<ChatMessageResult>> Handle(
        GetChatMessagesQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Security check
            var ownsSession = await _repo.ChatSessionBelongsToUser(
                request.ChatSessionId,
                request.UserId);

            if (!ownsSession)
                throw new UnauthorizedAccessException();

            var messages = await _repo.GetMessagesAsync(request.ChatSessionId);

            // AI failed to generate response
            if (messages == null || !messages.Any())
                throw new TimeoutException("The Model was unable to respond to the request");

            var results = new List<ChatMessageResult>();

            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            foreach (var m in messages)
            {
                var msgResult = new ChatMessageResult
                {
                    Id = m.Id,
                    Role = m.Role,
                    Content = m.Content,
                    CreatedAt = m.CreatedAt
                };

                // For assistant messages, try to attach saved columns/rows
                if (m.Role == "assistant")
                {
                    var meta = await _metadataRepo.GetByMessageIdAsync(m.Id);
                    if (meta != null && !string.IsNullOrEmpty(meta.LlmResponseJson))
                    {
                        try
                        {
                            using var doc = JsonDocument.Parse(meta.LlmResponseJson);
                            var root = doc.RootElement;

                            bool TryGetPropertyIgnoreCase(JsonElement el, string name, out JsonElement prop)
                            {
                                if (el.TryGetProperty(name, out prop))
                                    return true;

                                foreach (var p in el.EnumerateObject())
                                {
                                    if (string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase))
                                    {
                                        prop = p.Value;
                                        return true;
                                    }
                                }

                                prop = default;
                                return false;
                            }

                            if (TryGetPropertyIgnoreCase(root, "columns", out var cols))
                            {
                                var colsObj = JsonSerializer.Deserialize<object>(cols.GetRawText(), jsonOptions);
                                msgResult.GetType().GetProperty("Columns")?.SetValue(msgResult, colsObj);
                            }

                            if (TryGetPropertyIgnoreCase(root, "rows", out var rows))
                            {
                                var rowsObj = JsonSerializer.Deserialize<object>(rows.GetRawText(), jsonOptions);
                                msgResult.GetType().GetProperty("Rows")?.SetValue(msgResult, rowsObj);
                            }
                        }
                        catch { /* ignore parse errors */ }
                    }
                }

                results.Add(msgResult);
            }

            return results;
        }
        catch (TimeoutException)
        {
            throw; // handled by middleware → 504
        }
        catch (Exception)
        {
            throw new ApplicationException("System was unable to respond to the request");
        }
    }
}