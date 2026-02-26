using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Chat.Commands;
using AIChatbot.Application.Chat.Results;
using AIChatbot.Application.Common;
using AIChatbot.Application.RoleAccess.Services;
using AIChatbot.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AIChatbot.Application.Chat.Handlers;

public class RetryAssistantReplyHandler
    : IRequestHandler<RetryAssistantReplyCommand, ChatCommandResult>
{
    private readonly IChatSessionRepository _repo;
    private readonly IAiProviderService _ai;
    private readonly IRoleAccessService _roleAccessService;
    private readonly UserManager<ApplicationUser> _userManager;

    public RetryAssistantReplyHandler(
        IChatSessionRepository repo,
        IAiProviderService ai,
        IRoleAccessService roleAccessService,
        UserManager<ApplicationUser> userManager)
    {
        _repo = repo;
        _ai = ai;
        _roleAccessService = roleAccessService;
        _userManager = userManager;
    }

    public async Task<ChatCommandResult> Handle(
        RetryAssistantReplyCommand request,
        CancellationToken cancellationToken)
    {
        // 🔒 ensure session belongs to user
        var owns = await _repo.ChatSessionBelongsToUser(
            request.ChatSessionId,
            request.UserId);

        if (!owns)
            throw new UnauthorizedAccessException();

        // 🔍 load original user message
        var msg = await _repo.GetMessageAsync(request.MessageId);

        if (msg == null || msg.Role != "user")
            throw new InvalidOperationException("Only user messages can be retried.");

        try
        {
            // 🔐 get user + role
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
                throw new Exception("User not found");

            var roles = await _userManager.GetRolesAsync(user);
            var primaryRole = roles.FirstOrDefault();

            if (primaryRole == null)
                throw new Exception("User has no role assigned");

            // 🔐 load role access schema
            var roleAccess = await _roleAccessService.GetAccessAsync(primaryRole);

            // 🟢 load history
            var history = await _repo.GetLatestMessagesAsync(msg.ChatSessionId, 5);

            // 🧠 call AI WITH role schema
            var llm = await _ai.GetReplyAsync(
                msg.Content,
                history,
                roleAccess
            );

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