using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Chat.Commands;
using AIChatbot.Application.Chat.Results;
using AIChatbot.Application.Common;
using AIChatbot.Domain.Entities;

using MediatR;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace AIChatbot.Application.Chat.Handlers;

public class RetryAssistantReplyHandler
    : IRequestHandler<
        RetryAssistantReplyCommand,
        ChatCommandResult>
{
    private readonly
        IChatSessionRepository _repo;

    private readonly
        IAiProviderService _ai;

    private readonly
        UserManager<ApplicationUser>
        _userManager;

    private readonly
        RoleManager<IdentityRole>
        _roleManager;

    private readonly
        IRoleConnectionMappingRepository
        _mappingRepo;

    public RetryAssistantReplyHandler(
        IChatSessionRepository repo,

        IAiProviderService ai,

        UserManager<ApplicationUser> userManager,

        RoleManager<IdentityRole> roleManager,

        IRoleConnectionMappingRepository mappingRepo)
    {
        _repo = repo;

        _ai = ai;

        _userManager = userManager;

        _roleManager = roleManager;

        _mappingRepo = mappingRepo;
    }

    public async Task<ChatCommandResult>
        Handle(
            RetryAssistantReplyCommand request,
            CancellationToken cancellationToken)
    {
        // ====================================================
        // VALIDATE SESSION OWNER
        // ====================================================

        var owns =
            await _repo.ChatSessionBelongsToUser(
                request.ChatSessionId,
                request.UserId);

        if (!owns)
        {
            throw new UnauthorizedAccessException(
                "You do not have access to this session.");
        }

        // ====================================================
        // GET MESSAGE
        // ====================================================

        var msg =
            await _repo.GetMessageAsync(
                request.MessageId);

        if (msg == null
            ||
            msg.Role != "user")
        {
            throw new InvalidOperationException(
                "Only user messages can be retried.");
        }

        // ====================================================
        // USER
        // ====================================================

        var user =
            await _userManager
                .FindByIdAsync(
                    request.UserId)
            ??
            throw new KeyNotFoundException(
                "User not found.");

        // ====================================================
        // ROLE
        // ====================================================

        var roles =
            await _userManager
                .GetRolesAsync(user);

        var roleName =
            roles.FirstOrDefault()
            ??
            "User";

        // ====================================================
        // GET ROLE ENTITY
        // ====================================================

        var role =
            await _roleManager
                .FindByNameAsync(
                    roleName);

        // ====================================================
        // GET CONNECTION
        // ====================================================

        Guid connectionId =
            Guid.Empty;

        if (role != null)
        {
            var mappings =
                await _mappingRepo
                    .GetByRoleIdAsync(
                        role.Id);

            var mapping =
                mappings.FirstOrDefault();

            if (mapping != null)
            {
                connectionId =
                    mapping.ConnectionId;
            }
        }

        // ====================================================
        // HISTORY
        // ====================================================

        var history =
            await _repo.GetLatestMessagesAsync(
                msg.ChatSessionId,
                5);

        // ====================================================
        // AI CALL
        // ====================================================

        var llm =
            await _ai.GetReplyAsync(
                request.UserId,

                roleName,

                msg.Content,

                connectionId,

                history,

                null);

        // ====================================================
        // SAVE ASSISTANT MESSAGE
        // ====================================================

        await _repo.SaveMessageAsync(
            msg.ChatSessionId,

            "assistant",

            llm.Message ?? "");

        // ====================================================
        // RESPONSE
        // ====================================================

        return new ChatCommandResult
        {
            Status =
                ExecutionStatus.Success,

            ChatSessionId =
                msg.ChatSessionId,

            AssistantReply =
                llm.Message
        };
    }
}