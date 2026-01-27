using Agent.Application.AuthMediator.Command;
using Agent.Application.Dto.UserManagement;
using Agent.Application.Helpers;
using Agent.Application.Interface;
using Agent.Domain.Entities.UserManagement;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Application.Auth.Handler
{
    public class LoginHandler : IRequestHandler<LoginCommand, ApiResult<AuthTokenResponseDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuthService _authService;

        public LoginHandler(UserManager<ApplicationUser> userManager, IAuthService authService)
        {
            _userManager = userManager;
            _authService = authService;
        }

        public async Task<ApiResult<AuthTokenResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var dto = request.userLoginDto;
            var user = await _userManager.FindByEmailAsync(dto.Email);

            string message = "Invalid email or password.";
            if (user == null)
            {
                return ApiResult<AuthTokenResponseDto>.Fail(message);
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.userLoginDto.Password);

            if (!isPasswordValid)
            {
                return ApiResult<AuthTokenResponseDto>.Fail(message);
            }

            var data = await _authService.AuthTokenResponseAsync(user);
            return ApiResult<AuthTokenResponseDto>.Ok(data, "User Logged in Successfully.");
        }
    }
}
