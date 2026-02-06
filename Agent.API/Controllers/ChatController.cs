using Agent.API.Helpers;
using Agent.Application.AuthMediator.Command;
using Agent.Application.ChatMediator.Command;
using Agent.Application.Dto.UserManagement;
using Agent.Application.Helpers;
using Agent.Application.Validators.Chats;
using Azure;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace Agent.API.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize] // 🔐 Access token required
    public class ChatController : ControllerBase
    {

        private readonly IValidator<ChatRequestDto> _chatRequestValidate;
        private readonly IValidator<string> _chatSessionIdValidate;
        private readonly IMediator _mediator;
        public ChatController(IMediator mediator, IValidator<ChatRequestDto> chatRequestDto, IValidator<string> chatSessionIdValidate)
        {
            _mediator = mediator;
            _chatRequestValidate = chatRequestDto;
            _chatSessionIdValidate = chatSessionIdValidate;
        }

        [HttpGet("data")]
        public async Task<IActionResult> GetUserSessions()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(
                    ApiResult<SessionTitleResponseDto>.Fail("Unauthorized")
                );
            }


            ApiResult<SessionTitleResponseDto> response = await _mediator.Send(new SendUserSessionTitleCommand(userId));

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);

        }

        [HttpPost("chat")]

        public async Task<IActionResult> SendChatMessage(ChatRequestDto chatRequestDto)
        {
            var validationResult = await _chatRequestValidate.ValidateAsync(chatRequestDto);
            if (!validationResult.IsValid)
            {
                var problemDetails = ValidationProblemDetailsHelper.Build(validationResult, "Message Validation Failed");
                return BadRequest(problemDetails);
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(
                    ApiResult<ChatResponseDto>.Fail("Unauthorized")
                );
            }

            ApiResult<ChatResponseDto> response = await _mediator.Send(new SendChatMessageCommand(chatRequestDto, userId));

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpGet("message/{sessionId}")]

        public async Task<IActionResult> SendAllMessage(string sessionId)
        {
            var validationResult = await _chatSessionIdValidate.ValidateAsync(sessionId);

            if (!validationResult.IsValid)
            {
                return BadRequest(
                    ValidationProblemDetailsHelper.Build(
                        validationResult, "Session Validation Failed"));
            }

            ApiResult<ChatHistoryResponseDto> response = await _mediator.Send(new SendAllMessageCommand(sessionId));

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }


    }
}
