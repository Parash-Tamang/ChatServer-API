using AIChatbot.Application.Interfaces;
using AIChatbot.Api.Models;   // <-- for ChatRequest
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace AIChatbot.Api.Controllers;



[ApiController]
[Route("api/chat")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatSessionService _chatService;

    public ChatController(IChatSessionService chatService)
    {
        _chatService = chatService;
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? throw new Exception("User not authenticated");
    }

    // GET /api/chat
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();
        var sessions = await _chatService.GetAllAsync(userId);
        return Ok(sessions);
        
    }


    [HttpGet("{chatSessionId:guid}/messages")]
    public async Task<IActionResult> GetMessages(Guid chatSessionId)
    {
        var userId = GetUserId();

        var ownsSession = await _chatService
            .ChatSessionBelongsToUserAsync(chatSessionId, userId);

        if (!ownsSession)
            return Forbid();

        var messages = await _chatService.GetMessagesAsync(chatSessionId);
        return Ok(messages);
    }

    // POST /api/chat
    [HttpPost]
    public async Task<IActionResult> Send(ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest("Message cannot be empty");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var result = await _chatService.SendAsync(
            userId,
            request.ChatSessionId,
            request.Message
        );

        return Ok(new
        {
            chatSessionId = result.ChatSessionId,
            reply = result.Reply
        });
    }


    [HttpDelete("{chatSessionId:guid}")]
    public async Task<IActionResult> Delete(Guid chatSessionId) // Changed to public
    {
        var userId = GetUserId();
        await _chatService.DeleteAsync(chatSessionId, userId);
        return NoContent();
    }
}
