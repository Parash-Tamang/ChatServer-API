using Agent.Application.AuthMediator.Command;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Agent.Domain.Entities.UserManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agent.Application.Helpers;
using Agent.Application.Interface;

namespace Agent.Application.Auth.Handler
{
    public class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, ApiResult<object>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IAuthService _authService;


        public ResetPasswordHandler(UserManager<ApplicationUser> userManager, ITokenService tokenService, IAuthService authService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _authService = authService;
        }
        public async Task<ApiResult<object>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.userResetPasswordDto.Email);
            if (user == null)
            {
                return ApiResult<object>.Fail("Invalid Email Request");
            }
            var decodedToken = Uri.UnescapeDataString(request.userResetPasswordDto.Token);

            var result = await _userManager.ResetPasswordAsync(user,
                        decodedToken,
                        request.userResetPasswordDto.Password);

            if (!result.Succeeded)
            {
                
                return ApiResult<object>.Fail(
                    "Reset token is invalid or expired.");
            }

            await _tokenService.RevokeAllRefreshTokenAsync(user.Id); // revoke all the refresh token of the user
            //await _authService.AuthTokenResponseAsync(user);
            return ApiResult<object>.Ok(
                null,
                "Password reset successfully");

        }
    }
}
