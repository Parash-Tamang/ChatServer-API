using System.Security.Claims;

using AIChatbot.Api.Models.Chat;
using AIChatbot.Application.Chat.Commands;
using AIChatbot.Application.Chat.Queries;
using AIChatbot.Application.Common;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using AIChatbot.Application.Abstractions;

namespace AIChatbot.Api.Controllers;

[ApiController]

[Route("api/chat/V1/Conversation-engine")]

// ============================================================
// AUTHENTICATED USERS ONLY
// ============================================================

[Authorize(Policy = "AuthenticatedUser")]

// ============================================================
// CHAT RATE LIMITING
// ============================================================

[EnableRateLimiting("chat")]

public class ChatController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly
    IConnectionRepository _connectionRepo;

    private readonly
        IPromptFunctionRepository _promptRepo;

    private readonly
        ILocalFunctionRepository _localRepo;
    public ChatController(
      IMediator mediator,

      IConnectionRepository connectionRepo,

      IPromptFunctionRepository promptRepo,

      ILocalFunctionRepository localRepo)
    {
        _mediator = mediator;

        _connectionRepo = connectionRepo;

        _promptRepo = promptRepo;

        _localRepo = localRepo;
    }

    // ============================================================
    // RESOLVE AUTHENTICATED USER ID
    // ============================================================

    private string? UserId =>
        User.FindFirstValue(
            ClaimTypes.NameIdentifier)
        ??
        User.FindFirstValue("sub")
        ??
        User.FindFirstValue("userId");

    // ============================================================
    // GET ALL CHAT SESSIONS
    // ============================================================

    [HttpGet("Get/ChatSessions")]
    public async Task<IActionResult>
        GetAll()
    {
        if (UserId is null)
        {
            return Unauthorized(new
            {
                success = false,

                message =
                    "Unauthorized access."
            });
        }

        return Ok(await _mediator.Send(
            new GetChatSessionsQuery(
                UserId)));
    }

    // ============================================================
    // GET CHAT SESSION MESSAGES
    // ============================================================

    [HttpGet("{chatSessionId:guid}/messages")]
    public async Task<IActionResult>
        GetMessages(
            Guid chatSessionId)
    {
        if (UserId is null)
        {
            return Unauthorized(new
            {
                success = false,

                message =
                    "Unauthorized access."
            });
        }

        return Ok(await _mediator.Send(
            new GetChatMessagesQuery(
                UserId,
                chatSessionId)));
    }

    // ============================================================
    // SEND MESSAGE
    // ============================================================

    [HttpPost("Push-Query/Session!")]
    public async Task<IActionResult>
        Send(
            [FromBody]
            ChatRequest request)
    {
        if (UserId is null)
        {
            return Unauthorized(new
            {
                success = false,

                message =
                    "Unauthorized access."
            });
        }

        // ========================================================
        // BASIC REQUEST VALIDATION
        // ========================================================

        if (string.IsNullOrWhiteSpace(
                request.Message))
        {
            return BadRequest(new
            {
                success = false,

                message =
                    "Message cannot be empty."
            });
        }

        // ========================================================
        // MESSAGE SIZE LIMIT
        // ========================================================

        if (request.Message.Length > 10000)
        {
            return BadRequest(new
            {
                success = false,

                message =
                    "Message too large."
            });
        }

        var result =
            await _mediator.Send(
                new SendChatMessageCommand(
                    UserId,

                    request.ChatSessionId,

                    request.Message));

        return result.Status switch
        {
            ExecutionStatus.Success =>
                Ok(result),

            ExecutionStatus.PartiallyExecuted =>
                StatusCode(206, result),

            _ =>
                StatusCode(500, new
                {
                    success = false,

                    message =
                        "Unexpected AI execution failure."
                })
        };
    }

    // ============================================================
    // Excel Generate
    // ============================================================
    [HttpPost("excel/generate")]
    public async Task<IActionResult> GenerateExcel(
    [FromBody] GenerateExcelRequest request)
    {
        var bytes =
            await _mediator.Send(
                new GenerateExcelCommand(
                    request.MessageId));

        return File(
            bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Report_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx");
    }

    // ============================================================
    // RETRY ASSISTANT RESPONSE
    // ============================================================

    [HttpPost(
        "{chatSessionId:guid}/messages/{messageId:guid}/retry")]
    public async Task<IActionResult>
        Retry(
            Guid chatSessionId,

            Guid messageId)
    {
        if (UserId is null)
        {
            return Unauthorized(new
            {
                success = false,

                message =
                    "Unauthorized access."
            });
        }

        var result =
            await _mediator.Send(
                new RetryAssistantReplyCommand(
                    UserId,
                    chatSessionId,
                    messageId));

        return result.Status switch
        {
            ExecutionStatus.Success =>
                Ok(result),

            ExecutionStatus.PartiallyExecuted =>
                StatusCode(206, result),

            _ =>
                StatusCode(500, new
                {
                    success = false,

                    message =
                        "Unexpected AI execution failure."
                })
        };
    }

    // ============================================================
    // DELETE CHAT SESSION
    // ============================================================

    [HttpDelete("{chatSessionId:guid}/Delete")]
    public async Task<IActionResult>
        Delete(
            Guid chatSessionId)
    {
        if (UserId is null)
        {
            return Unauthorized(new
            {
                success = false,

                message =
                    "Unauthorized access."
            });
        }

        await _mediator.Send(
            new DeleteChatSessionCommand(
                UserId,
                chatSessionId));

        return NoContent();
    }




    // 
}