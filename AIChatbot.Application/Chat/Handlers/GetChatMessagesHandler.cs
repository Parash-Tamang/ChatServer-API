using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Chat.Queries;
using AIChatbot.Application.Chat.Results;
using MediatR;

namespace AIChatbot.Application.Chat.Handlers;

/// Retrieves all messages for a chat session
public class GetChatMessagesHandler
    : IRequestHandler<GetChatMessagesQuery, IReadOnlyList<ChatMessageResult>>
{
    private readonly IChatSessionRepository _repo;
    private readonly IResponseMetadataRepository _metadataRepo;

    public GetChatMessagesHandler(
        IChatSessionRepository repo,
        IResponseMetadataRepository metadataRepo)
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
            // ============================================
            // SECURITY CHECK
            // ============================================

            var ownsSession =
                await _repo.ChatSessionBelongsToUser(
                    request.ChatSessionId,
                    request.UserId);

            if (!ownsSession)
            {
                throw new UnauthorizedAccessException(
                    "Access denied.");
            }

            // ============================================
            // LOAD MESSAGES
            // ============================================

            var messages =
                await _repo.GetMessagesAsync(
                    request.ChatSessionId);

            if (messages == null || !messages.Any())
            {
                throw new TimeoutException(
                    "The Model was unable to respond to the request");
            }

            var results =
                new List<ChatMessageResult>();

            // ============================================
            // BUILD RESPONSE
            // ============================================

            foreach (var m in messages)
            {
                ChatMessageResult msgResult;

                if (m.Role == "assistant")
                {
                    var meta =
                        await _metadataRepo.GetByMessageIdAsync(
                            m.Id);

                    string? graphImageBase64 = null;

                    if (!string.IsNullOrWhiteSpace(
                            meta?.GraphImagePath))
                    {
                        var physicalPath =
                            Path.Combine(
                                Directory.GetCurrentDirectory(),
                                "wwwroot",
                                meta.GraphImagePath);

                        if (File.Exists(physicalPath))
                        {
                            var imageBytes =
                                await File.ReadAllBytesAsync(
                                    physicalPath,
                                    cancellationToken);

                            graphImageBase64 =
                                Convert.ToBase64String(
                                    imageBytes);
                        }
                    }

                    msgResult = new ChatMessageResult
                    {
                        Id = m.Id,

                        Role = m.Role,

                        Content = m.Content,

                        CreatedAt = m.CreatedAt,

                        ExcelGenerated =
                            meta?.ExcelGenerated ?? false,

                        // Excel is regenerated later via API
                        ExcelAvailableNow = null,

                        GraphType =
                            meta?.GraphType,

                        GraphTitle =
                            meta?.GraphTitle,

                        GraphImageUrl =
                            meta?.GraphImageUrl,

                        GraphImageBase64 =
                            graphImageBase64,

                        HasGraph =
                            !string.IsNullOrWhiteSpace(
                                meta?.GraphImageUrl)
                    };
                }
                else
                {
                    msgResult = new ChatMessageResult
                    {
                        Id = m.Id,

                        Role = m.Role,

                        Content = m.Content,

                        CreatedAt = m.CreatedAt,

                        ExcelGenerated = false,

                        ExcelAvailableNow = null,

                        GraphType = null,

                        GraphTitle = null,

                        GraphImageUrl = null,

                        GraphImageBase64 = null,

                        HasGraph = false
                    };
                }

                results.Add(msgResult);
            }

            return results;
        }
        catch (TimeoutException)
        {
            throw;
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ApplicationException(
                $"System was unable to respond to the request. {ex.Message}");
        }
    }
}