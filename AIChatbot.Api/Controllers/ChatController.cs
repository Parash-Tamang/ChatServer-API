using AIChatbot.Api.Models.Chat;
using AIChatbot.Application.Chat.Commands;
using AIChatbot.Application.Chat.Queries;
using AIChatbot.Application.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AIChatbot.Api.Controllers;

[ApiController]
[Route("api/chat")]
[Authorize] // 🔐 Chat APIs must be authenticated
public class ChatController : ControllerBase
{
    private readonly IMediator _mediator;

    public ChatController(IMediator mediator)
    {
        _mediator = mediator;
    }

    
    /// Safely resolves authenticated user id from JWT
  
    private string? UserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ??
        User.FindFirstValue("sub") ??
        User.FindFirstValue("userId");




    // API/CHAT/GETALL { for getting all chat sessions of the user }
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        if (UserId is null)
            return Unauthorized();

        return Ok(await _mediator.Send(
            new GetChatSessionsQuery(UserId)));
    }



    // API/CHAT/GETMESSAGES { for getting all messages of a specific chat session }

    [HttpGet("{chatSessionId:guid}/messages")]
    public async Task<IActionResult> GetMessages(Guid chatSessionId)
    {
        if (UserId is null)
            return Unauthorized();

        return Ok(await _mediator.Send(
            new GetChatMessagesQuery(UserId, chatSessionId)));
    }



    // API/CHAT/GETLATEST { for getting latest N messages of a specific chat session }

    [HttpGet("{chatSessionId:guid}/messages/latest/{count:int}")]
    public async Task<IActionResult> GetLatest(Guid chatSessionId, int count)
    {
        if (UserId is null)
            return Unauthorized();

        return Ok(await _mediator.Send(
            new GetLatestChatMessagesQuery(UserId, chatSessionId, count)));
    }



    // API/CHAT/SEND { for sending a new message to a specific chat session or if  chatsession id == null create new chat session }
    [HttpPost]
    public async Task<IActionResult> Send([FromBody] ChatRequest request)
    {
        if (UserId is null)
            return Unauthorized();

        var result = await _mediator.Send(
            new SendChatMessageCommand(
                UserId,
                request.ChatSessionId,
                request.Message));

        return result.Status switch
        {
            ExecutionStatus.Success => Ok(result),
            ExecutionStatus.PartiallyExecuted => StatusCode(206, result),
            _ => StatusCode(500, result)
        };
    }



    // API/CHAT/RETRY { for retrying assistant reply to a specific user message }
    [HttpPost("{chatSessionId:guid}/messages/{messageId:guid}/retry")]
    public async Task<IActionResult> Retry(Guid chatSessionId, Guid messageId)
    {
        if (UserId is null)
            return Unauthorized();

        var result = await _mediator.Send(
            new RetryAssistantReplyCommand(UserId, chatSessionId, messageId));

        return result.Status switch
        {
            ExecutionStatus.Success => Ok(result),
            ExecutionStatus.PartiallyExecuted => StatusCode(206, result),
            _ => StatusCode(500, result)
        };
    }


    // API/CHAT/DELETE { for deleting a specific chat session }

    [HttpDelete("{chatSessionId:guid}")]
    public async Task<IActionResult> Delete(Guid chatSessionId)
    {
        if (UserId is null)
            return Unauthorized();

        await _mediator.Send(
            new DeleteChatSessionCommand(UserId, chatSessionId));

        return NoContent();
    }


  
    /// Debug endpoint to inspect JWT claims
   
    [HttpGet("whoami")]
    public IActionResult WhoAmI()
        => Ok(new
        {
            IsAuthenticated = User.Identity?.IsAuthenticated,
            Claims = User.Claims.Select(c => new { c.Type, c.Value })
        });
}
