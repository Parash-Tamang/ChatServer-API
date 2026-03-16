using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Chat.Commands;
using AIChatbot.Application.Chat.Results;
using AIChatbot.Application.Common;
using AIChatbot.Application.RoleAccess.Services;
using AIChatbot.Domain.Entities;
using MediatR;
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
        var sessionId = request.ChatSessionId
            ?? await _repo.CreateChatSessionAsync(request.UserId);

        bool isNewSession = request.ChatSessionId == null;

        try
        {
            // 🔐 1. Validate user
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
                throw new Exception("User not found.");

            // 🔐 2. Validate role
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Any())
                throw new Exception("User has no role assigned.");

            var primaryRole = roles.First();

            // 🔐 3. Get role access restrictions
            var roleAccess = await _roleAccessService.GetAccessAsync(primaryRole);

            // 🟢 4. Load history
            var history = await _repo.GetLatestMessagesAsync(sessionId, 5);

            // 🟢 5. Call AI
            var llm = await _ai.GetReplyAsync(
                request.Message,
                history,
                roleAccess
            );

            // 🟢 6. Save USER message
            var userMessageId = await _repo.SaveMessageAsync(
                sessionId,
                "user",
                request.Message
            );

            // 🟢 7. Auto topic generation
            if (isNewSession)
            {
                var topic = string.Join(" ",
                    request.Message
                        .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .TakeLast(4));

                await _namingRepo.CreateAsync(new ChatSessionNaming
                {
                    ChatSessionId = sessionId,
                    MessageId = userMessageId,
                    TopicName = topic
                });
            }

            // 🟢 8. Save assistant reply
            var assistantMessageId = await _repo.SaveMessageAsync(
                sessionId,
                "assistant",
                llm.Message ?? ""
            );

            // 🟢 9. Save metadata
            await _metadataRepo.SaveAsync(new ResponseMetadata
            {
                MessageId = assistantMessageId,
                Success = llm.Success,
                Query = request.Message,
                InfoMessage = llm.Message,
                SqlGenerated = llm.SqlGenerated,
                LlmResponseJson = JsonSerializer.Serialize(llm),
                RowCount = llm.RowCount,
                WasReconstructed = llm.WasReconstructed,
                ClarificationNeeded = llm.ClarificationNeeded,
                CreatedAt = DateTime.UtcNow
            });

            return new ChatExecutionResult
            {
                Status = ExecutionStatus.Success,
                ChatSessionId = sessionId,
                MessageId = assistantMessageId,
                AssistantReply = llm.Message
            };
        }
        catch (TimeoutException)
        {
            // AI timeout → middleware will return 504
            throw;
        }
        catch (ApplicationException)
        {
            // System error → middleware will return 500
            throw;
        }
        catch (Exception)
        {
            // unexpected failure
            throw new ApplicationException("System was unable to respond to the request");
        }
    }
}