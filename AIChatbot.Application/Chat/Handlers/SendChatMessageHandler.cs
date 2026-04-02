using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Chat.Commands;
using AIChatbot.Application.Chat.Results;
using AIChatbot.Application.Common;
using AIChatbot.Application.RoleAccess.Services;
using AIChatbot.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;

namespace AIChatbot.Application.Chat.Handlers;

public class SendChatMessageHandler
    : IRequestHandler<SendChatMessageCommand, ChatExecutionResult>
{
    private readonly IChatSessionRepository _repo;
    private readonly IAiProviderService _ai;
    private readonly IResponseMetadataRepository _metadataRepo;
    private readonly IChatSessionNamingRepository _namingRepo;
    private readonly IRoleAccessService _roleAccessService;
    private readonly UserManager<ApplicationUser> _userManager;

    public SendChatMessageHandler(
        IChatSessionRepository repo,
        IAiProviderService ai,
        IResponseMetadataRepository metadataRepo,
        IChatSessionNamingRepository namingRepo,
        IRoleAccessService roleAccessService,
        UserManager<ApplicationUser> userManager)
    {
        _repo = repo;
        _ai = ai;
        _metadataRepo = metadataRepo;
        _namingRepo = namingRepo;
        _roleAccessService = roleAccessService;
        _userManager = userManager;
    }

    public async Task<ChatExecutionResult> Handle(
        SendChatMessageCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            throw new BadHttpRequestException("Message cannot be empty.");

        var sessionId = request.ChatSessionId
            ?? await _repo.CreateChatSessionAsync(request.UserId);

        var user = await _userManager.FindByIdAsync(request.UserId)
            ?? throw new KeyNotFoundException("User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault()
            ?? throw new BadHttpRequestException("User has no role assigned.");

        var roleAccess = await _roleAccessService.GetAccessAsync(role);

        var history = await _repo.GetLatestMessagesAsync(sessionId, 5);

        var llm = await _ai.GetReplyAsync(
            request.Message,
            history,
            roleAccess);

        var userMessageId = await _repo.SaveMessageAsync(
            sessionId,
            "user",
            request.Message);

        var assistantMessageId = await _repo.SaveMessageAsync(
            sessionId,
            "assistant",
            llm.Message ?? "");

        await _metadataRepo.SaveAsync(new ResponseMetadata
        {
            MessageId = assistantMessageId,
            Success = llm.Success,
            Query = request.Message,
            InfoMessage = llm.Message,
            SqlGenerated = llm.SqlGenerated,
            LlmResponseJson = JsonSerializer.Serialize(llm),
            RowCount = llm.RowCount,
            ConnectionStringId = llm.ConnectionStringId,
            CreatedAt = DateTime.UtcNow
        });

        return new ChatExecutionResult
        {
            Status = ExecutionStatus.Success,
            ChatSessionId = sessionId,
            MessageId = assistantMessageId,
            AssistantReply = llm.Message,
            Columns = llm.Columns ?? Array.Empty<object>(),
            Rows = llm.Rows ?? Array.Empty<object>()
        };
    }
}