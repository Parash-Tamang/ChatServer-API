using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Chat.Commands;
using AIChatbot.Application.Chat.Results;
using AIChatbot.Application.Common;
using AIChatbot.Domain.Entities;
using MediatR;
namespace AIChatbot.Application.Chat.Handlers;
public class RetryAssistantReplyHandler
    : IRequestHandler<RetryAssistantReplyCommand, ChatCommandResult>
{
    private readonly IChatSessionRepository _repo;
    private readonly IAiProviderService _ai;

    public RetryAssistantReplyHandler(
        IChatSessionRepository repo,
        IAiProviderService ai)
    {
        _repo = repo;
        _ai = ai;
    }

    public async Task<ChatCommandResult> Handle(
        RetryAssistantReplyCommand request,
        CancellationToken cancellationToken)
    {
        // 🔒 security check
        var owns = await _repo.ChatSessionBelongsToUser(
            request.ChatSessionId,
            request.UserId);

        if (!owns)
            throw new UnauthorizedAccessException();

        // load message
        var msg = await _repo.GetMessageAsync(request.MessageId);

        if (msg == null || msg.Role != "user")
            throw new InvalidOperationException("Only user messages can be retried.");

        try
        {
            // 🟢 load history (last 5)
            var history = await _repo.GetLatestMessagesAsync(msg.ChatSessionId, 5);

            // 🟢 call LLM correctly
            var llm = await _ai.GetReplyAsync(msg.Content, history);

            // 🟢 save assistant reply
            await _repo.SaveMessageAsync(
                msg.ChatSessionId,
                "assistant",
                llm.Message ?? ""
            );

            return new ChatCommandResult
            {
                Status = ExecutionStatus.Success,
                ChatSessionId = msg.ChatSessionId,
                AssistantReply = llm.Message
            };
        }
        catch
        {
            return new ChatCommandResult
            {
                Status = ExecutionStatus.PartiallyExecuted,
                ChatSessionId = msg.ChatSessionId
            };
        }
    }
}
