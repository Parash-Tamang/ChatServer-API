using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Chat.Commands;
using AIChatbot.Application.Chat.Results;
using AIChatbot.Application.Common;
using AIChatbot.Domain.Entities;

using MediatR;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

using System.Text.Json;
using System.IO;
namespace AIChatbot.Application.Chat.Handlers;

public class SendChatMessageHandler
    : IRequestHandler<
        SendChatMessageCommand,
        ChatExecutionResult>
{
    private readonly
        IChatSessionRepository _repo;

    private readonly
        IAiProviderService _ai;

    private readonly
        IResponseMetadataRepository
        _metadataRepo;

    private readonly
        IChatSessionNamingRepository
        _namingRepo;

    private readonly
        UserManager<ApplicationUser>
        _userManager;

    private readonly
        RoleManager<IdentityRole>
        _roleManager;

    private readonly
        IRoleConnectionMappingRepository
        _mappingRepo;

    public SendChatMessageHandler(
        IChatSessionRepository repo,

        IAiProviderService ai,

        IResponseMetadataRepository metadataRepo,

        IChatSessionNamingRepository namingRepo,

        UserManager<ApplicationUser> userManager,

        RoleManager<IdentityRole> roleManager,

        IRoleConnectionMappingRepository mappingRepo)
    {
        _repo = repo;

        _ai = ai;

        _metadataRepo = metadataRepo;

        _namingRepo = namingRepo;

        _userManager = userManager;

        _roleManager = roleManager;

        _mappingRepo = mappingRepo;
    }

    public async Task<ChatExecutionResult>
        Handle(
            SendChatMessageCommand request,
            CancellationToken cancellationToken)
    {
        // ====================================================
        // VALIDATE
        // ====================================================

        if (string.IsNullOrWhiteSpace(
                request.Message))
        {
            throw new BadHttpRequestException(
                "Message cannot be empty.");
        }

        // ====================================================
        // SESSION
        // ====================================================

        bool isNewSession =
        request.ChatSessionId == null;

        var sessionId =
            request.ChatSessionId
            ??
            await _repo.CreateChatSessionAsync(
                request.UserId);
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
            ?? "User";

        var role =
            await _roleManager
                .FindByNameAsync(roleName);

        var roleId =
            role?.Id;
        // ====================================================
        // GET CONNECTION
        // ====================================================

        Guid connectionId =
            Guid.Empty;

        if (role != null)
        {
            var mappings =
                await _mappingRepo
                    .GetByRoleIdAsync(role.Id);

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
                sessionId,
                5);

        var sessionContext =
   new Dictionary<string, object?>
   {
       ["session_id"] =
           sessionId.ToString(),

       ["db_id"] =
           connectionId.ToString(),

       ["chat_id"] =
           sessionId.ToString(),

       ["last_refined_query"] = "",

       ["last_intent"] = "",

       ["last_confirmed_sql"] = "",

       ["last_tables_used"] =
           Array.Empty<string>(),

       ["last_filters"] =
           new Dictionary<string, object>(),

       ["last_skeleton_id"] = 0,

       ["turn_count"] = 1,

       ["created_at"] =
           DateTime.UtcNow,

       ["updated_at"] =
           DateTime.UtcNow
   };

        // ====================================================
        // AI CALL
        // ====================================================

        var llm =
            await _ai.GetReplyAsync(
                request.UserId,

                roleName,

                request.Message,

                connectionId,

                history,

               sessionContext);


        // ====================================================
        // SAVE USER MESSAGE
        // ====================================================

        var userMessageId =
            await _repo.SaveMessageAsync(
                sessionId,

                "user",

                request.Message);
        // ====================================================
        // CREATE CHAT TITLE
        // ====================================================

        if (isNewSession)
        {
            var words =
                request.Message
                    .Split(
                        ' ',
                        StringSplitOptions.RemoveEmptyEntries);

            var topicName =
                string.Join(
                    " ",
                    words.Take(5));

            await _namingRepo.CreateAsync(
                new ChatSessionNaming
                {
                    Id = Guid.NewGuid(),

                    ChatSessionId = sessionId,

                    MessageId = userMessageId,

                    TopicName = topicName,

                    CreatedAt = DateTime.UtcNow
                });
        }

        // ====================================================
        // SAVE ASSISTANT MESSAGE
        // ====================================================

        var assistantMessageId =
            await _repo.SaveMessageAsync(
                sessionId,
                "assistant",
                llm.Message ?? "");

        // ====================================================
        // SAVE GRAPH IMAGE
        // ====================================================

        string? graphImageUrl = null;
        string? graphImagePath = null;
        string? graphImageBase64 = null;

        if (!string.IsNullOrWhiteSpace(llm.GraphImageBase64))
        {
            var fileName =
                $"{Guid.NewGuid()}.png";

            var relativePath =
                Path.Combine(
                    "graph-images",
                    fileName);

            var physicalPath =
                Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    relativePath);

            Directory.CreateDirectory(
                Path.GetDirectoryName(
                    physicalPath)!);

            var bytes =
                Convert.FromBase64String(
                    llm.GraphImageBase64);

            await File.WriteAllBytesAsync(
                physicalPath,
                bytes);

            graphImagePath =
                relativePath.Replace("\\", "/");

            graphImageUrl =
                "/" + graphImagePath;
        }
        if (!string.IsNullOrWhiteSpace(graphImagePath))
        {
            var physicalPath =
                Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    graphImagePath);

            if (File.Exists(physicalPath))
            {
                graphImageBase64 =
                    Convert.ToBase64String(
                        await File.ReadAllBytesAsync(
                            physicalPath,
                            cancellationToken));
            }
        }

        // ====================================================
        // SAVE METADATA
        // ====================================================

        await _metadataRepo.SaveAsync(
            new ResponseMetadata
            {
                MessageId =
                    assistantMessageId,

                ConnectionStringId =
                    llm.ConnectionStringId,

                RoleUsed =
                    roleId,

                Success =
                    llm.Success,

                Query =
                    request.Message,

                RefinedQuery =
                    llm.RefinedQuery,

                IntentDetected =
                    llm.IntentDetected,

                InfoMessage =
                    llm.Message,

                SqlGenerated =
                    llm.SqlGenerated,

                ExcelGenerated =
                    llm.ExcelGenerated,

                RowCount =
                    llm.RowCount,

                WasReconstructed =
                    llm.WasReconstructed,

                ClarificationNeeded =
                    llm.ClarificationNeeded,

                TablesUsedJson =
                    llm.TablesUsed == null
                        ? null
                        : JsonSerializer.Serialize(
                            llm.TablesUsed),

                FiltersJson =
                    llm.Filters == null
                        ? null
                        : JsonSerializer.Serialize(
                            llm.Filters),

                SessionContextJson =
    llm.SessionContext?.ToString(),

                GraphType =
                    llm.GraphType,

                GraphTitle =
                    llm.GraphTitle,

                GraphReasoning =
                    llm.GraphReasoning,

                GraphImageUrl =
                    graphImageUrl,

                GraphImagePath =
                    graphImagePath,

                FullResponseJson =
                    llm.FullResponseJson,

                LlmResponseJson =
                    JsonSerializer.Serialize(llm),

                CreatedAt =
                    DateTime.UtcNow
            });

        // ====================================================
        // RESPONSE
        // ====================================================

        return new ChatExecutionResult
        {
            Status = ExecutionStatus.Success,

            ChatSessionId =
                sessionId,

            MessageId =
                assistantMessageId,

            AssistantReply =
                llm.Message,

            ExcelGenerated =
                llm.ExcelGenerated,

            ExcelAvailableNow =
                string.IsNullOrWhiteSpace(
                    llm.ExcelJson)
                        ? null
                        : JsonSerializer.Deserialize<object>(
                            llm.ExcelJson),

            GraphType =
                llm.GraphType,

            GraphTitle =
                llm.GraphTitle,

            GraphImageUrl =
        graphImageUrl,

            GraphImageBase64 =
        graphImageBase64
        };
    }
}