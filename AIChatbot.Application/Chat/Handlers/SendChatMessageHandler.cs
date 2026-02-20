using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Chat.Commands;
using AIChatbot.Application.Chat.Results;
using AIChatbot.Application.Common;
using AIChatbot.Domain.Entities;
using MediatR;
using System.Text.Json;

namespace AIChatbot.Application.Chat.Handlers;

public class SendChatMessageHandler
    : IRequestHandler<SendChatMessageCommand, ChatExecutionResult>
{
    private readonly IChatSessionRepository _repo;
    private readonly IAiProviderService _ai;
    private readonly IResponseMetadataRepository _metadataRepo;
    private readonly IChatSessionNamingRepository _namingRepo;

    public SendChatMessageHandler(
        IChatSessionRepository repo,
        IAiProviderService ai,
        IResponseMetadataRepository metadataRepo,
        IChatSessionNamingRepository namingRepo)
    {
        _repo = repo;
        _ai = ai;
        _metadataRepo = metadataRepo;
        _namingRepo = namingRepo;
    }

    public async Task<ChatExecutionResult> Handle(
        SendChatMessageCommand request,
        CancellationToken cancellationToken)
    {
        // 🔵 detect new session
        bool isNewSession = request.ChatSessionId == null;

        var sessionId = request.ChatSessionId
            ?? await _repo.CreateChatSessionAsync(request.UserId);

        try
        {
            // 🟢 load last 5 history BEFORE saving new message
            var history = await _repo.GetLatestMessagesAsync(sessionId, 5);

            // 🟢 call LLM
            var llm = await _ai.GetReplyAsync(request.Message, history);

            // 🟢 save USER message
            var userMessageId = await _repo.SaveMessageAsync(
                sessionId,
                "user",
                request.Message
            );

            // 🟢 if new chat session → create topic name
            if (isNewSession)
            {
                var words = request.Message
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .TakeLast(4);

                var topic = string.Join(" ", words);

                await _namingRepo.CreateAsync(new ChatSessionNaming
                {
                    ChatSessionId = sessionId,
                    MessageId = userMessageId,
                    TopicName = topic
                });
            }

            // 🟢 save assistant reply
            var assistantMessageId = await _repo.SaveMessageAsync(
                sessionId,
                "assistant",
                llm.Message ?? ""
            );

            // 🟢 save response metadata
            await _metadataRepo.SaveAsync(new ResponseMetadata
            {
                MessageId = assistantMessageId,
                Success = llm.Success,
                Query = request.Message,
                InfoMessage = llm.Message,
                SqlGenerated = llm.SqlGenerated,
                ColumnsJson = llm.Columns != null ? JsonSerializer.Serialize(llm.Columns) : null,
                RowsJson = llm.Rows != null ? JsonSerializer.Serialize(llm.Rows) : null,
                RowCount = llm.RowCount,
                WasReconstructed = llm.WasReconstructed,
                ClarificationNeeded = llm.ClarificationNeeded,
                TokenUsageJson = llm.TokenUsage != null ? JsonSerializer.Serialize(llm.TokenUsage) : null
            });

            return new ChatExecutionResult
            {
                Status = ExecutionStatus.Success,
                ChatSessionId = sessionId,
                MessageId = assistantMessageId,   // 🔥 RETURNING MESSAGE ID
                AssistantReply = llm.Message
            };

        }
        catch (Exception ex)
        {
            Console.WriteLine("CHAT ERROR:");
            Console.WriteLine(ex);

            return new ChatExecutionResult
            {
                Status = ExecutionStatus.PartiallyExecuted,
                ChatSessionId = sessionId
            };
        }
    }
}
