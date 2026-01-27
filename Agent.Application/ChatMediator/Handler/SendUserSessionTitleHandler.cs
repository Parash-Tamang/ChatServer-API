using Agent.Application.ChatMediator.Command;
using Agent.Application.Dto.UserManagement;
using Agent.Application.Helpers;
using Agent.Application.Interface;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Application.ChatMediator.Handler
{
    public class SendUserSessionTitleHandler : IRequestHandler<SendUserSessionTitleCommand, ApiResult<SessionTitleResponseDto>>
    {

        private readonly IChatSessionService _chatSessionService;

        public SendUserSessionTitleHandler(IChatSessionService chatSessionService)
        {
            _chatSessionService = chatSessionService;
        }
        public async Task<ApiResult<SessionTitleResponseDto>> Handle(SendUserSessionTitleCommand request, CancellationToken cancellationToken)
        {
            SessionTitleResponseDto data = await _chatSessionService.GetUserSessionsAsync(request.userId);

            if(data.sessionTitleList == null || !data.sessionTitleList.Any())
            {
                return ApiResult<SessionTitleResponseDto>.Ok(data, "No sessions found.");
            }
            return ApiResult<SessionTitleResponseDto>.Ok(data,"Session titles fetched successfully.");
        }
    }
}
