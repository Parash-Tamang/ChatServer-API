using Agent.Application.ChatMediator.Command;
using Agent.Application.Dto.UserManagement;
using Agent.Application.Helpers;
using Agent.Application.Interface;
using MediatR;
using Agent.Domain.Entities.UserManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Application.ChatMediator.Handler
{
    public class SendAllMessageHandler : IRequestHandler<SendAllMessageCommand,ApiResult<ChatHistoryResponseDto>>
    {
        private readonly IChatSessionService _chatSessionService;
        public SendAllMessageHandler(IChatSessionService chatSessionService) 
        {
                _chatSessionService = chatSessionService;
        }

        public async Task<ApiResult<ChatHistoryResponseDto>>Handle(SendAllMessageCommand request, CancellationToken cancellationToken)
        {
            UserSession? session = await _chatSessionService.GetExistAsync(request.sessionId);

            if (session == null)
            {
                return ApiResult<ChatHistoryResponseDto>.Fail("Session does not exist.");
            }

            var messages = await _chatSessionService.GetChatMessageBySessionId(request.sessionId);

            var data = new ChatHistoryResponseDto
            {
                SessionId = session.SessionId,
                SessionTitle = session.Title, // fetch from session table if needed
                Messages = messages
            };

            return ApiResult<ChatHistoryResponseDto>.Ok(data, "Message Fetched Successfully.");
        }

    }
}
