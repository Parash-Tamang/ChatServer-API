using Agent.Application.ChatMediator.Command;
using Agent.Application.Dto.UserManagement;
using Agent.Application.Helpers;
using Agent.Application.Interface;
using Agent.Domain.Entities.UserManagement;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
namespace Agent.Application.ChatMediator.Handler
{
    public class SendChatMessageHandler : IRequestHandler<SendChatMessageCommand, ApiResult<ChatResponseDto>>
    {
        private readonly IChatSessionService _chatSessionService;
        private readonly IChatMessageService _chatMessageService;
        private readonly IAgentService _agentService;

        public SendChatMessageHandler(IChatSessionService chatSessionService, IChatMessageService chatMessageService, IAgentService agentService)
        {
            _chatSessionService = chatSessionService;
            _chatMessageService = chatMessageService;
            _agentService = agentService;
        }

        public async Task<ApiResult<ChatResponseDto>> Handle(SendChatMessageCommand request, CancellationToken cancellationToken)
        {
            UserSession? session;
            string sessionId;
            string title;

            // Case 1 : new chat - sessionId = null 
            if (request.chatRequestDto.sessionId == null)
            {
                 session = await _chatSessionService.CreateChatSessionAsync(request.chatRequestDto.message,request.userId);
                 sessionId = session.SessionId;
                 title = session.Title;
            }
            else
            {
                // CASE 2: sessionId exists → validate

                session = await _chatSessionService.GetExistAsync(request.chatRequestDto.sessionId);
                if (session == null)
                {
                    return ApiResult<ChatResponseDto>.Fail(
                        "Chat session not found"
                    );
                }
                sessionId = request.chatRequestDto.sessionId;
                title = session.Title;
            }

            // Save user message
            await _chatMessageService.StoreChatMessageAsync(new UserMessageRequestDto 
            { MessageText = request.chatRequestDto.message,
              SenderType = "user",
              SessionId = sessionId,
            });

            // Generate reply
            var reply = _agentService.GenerateAsync(sessionId);

            if (string.IsNullOrWhiteSpace(reply))
            {
                return ApiResult<ChatResponseDto>.Fail(
                    "Failed to generate a response. Please try again."
                );
            }

            // Save assistant message
            await _chatMessageService.StoreChatMessageAsync(new UserMessageRequestDto
            {
                MessageText = reply,
                SenderType = "assistant",
                SessionId = sessionId,
            });

            ChatResponseDto data = new ChatResponseDto 
            { 
                SessionId = sessionId,
                SessionTitle = title,
                AssistantMessage = new UserMessageResponseDto
                {
                    SessionId = sessionId,
                    MessageText = reply,
                    SenderType = "assistant"
    }
            };
            return ApiResult<ChatResponseDto>.Ok(data,"Message send successfully");
        }
    }
}
