using Agent.Application.AuthMediator.Command;
using Agent.Application.Dto.UserManagement;
using Agent.Application.Helpers;
using Agent.Application.Interface;
using Agent.Domain.Entities.UserManagement;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Agent.Application.Auth.Handler
{
    public class LogoutHandler : IRequestHandler<LogoutCommand, ApiResult<object>>
    {
        private readonly ITokenService _tokenService;

        public LogoutHandler(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        public async Task<ApiResult<object>> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            
            await _tokenService.RevokeAsync(request.refreshTokenRequestDto);

            return ApiResult<object>.Ok(null, "User Logout Successfully");
        }
    }
}
