using Agent.Application.AuthMediator.Command;
using Agent.Application.Dto.UserManagement;
using Agent.Application.Helpers;
using Agent.Application.Interface;
using Agent.Domain.Entities.UserManagement;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
namespace Agent.Application.Auth.Handler
{
    public class RefreshTokenHandler
     : IRequestHandler<RefreshTokenCommand, ApiResult<AuthTokenResponseDto>>
    {
        
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;

        public RefreshTokenHandler(UserManager<ApplicationUser> userManager, IAuthService authTokenService, ITokenService tokenService)
        {
            _userManager = userManager;
            _authService = authTokenService;
            _tokenService = tokenService;
        }

        public async Task<ApiResult<AuthTokenResponseDto>> Handle(RefreshTokenCommand request,CancellationToken cancellationToken)
        {

            RefreshToken refreshToken;

            try
            {
                refreshToken = await _tokenService.ValidateAndConsumeRefreshTokenAsync(request.refreshTokenRequestDto.RefreshToken);
            }
            catch (SecurityTokenException)
            {
                return ApiResult<AuthTokenResponseDto>
                    .Fail("Invalid or expired refresh token");
            }
            // 2️⃣ Load user
            var user = await _userManager
                .FindByIdAsync(refreshToken.UserId);
            
            if (user == null)
            {
                return ApiResult<AuthTokenResponseDto>
                    .Fail("User not found");
            }

            // 3️⃣ Generate new tokens
            var tokens = await _authService
                .AuthTokenResponseAsync(user);

            // 4️⃣ Return response
            return ApiResult<AuthTokenResponseDto>
                .Ok(tokens, "Token refreshed successfully");
        }
    }
}
