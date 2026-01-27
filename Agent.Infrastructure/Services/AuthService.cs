using Agent.Application.Dto.UserManagement;
using Agent.Application.Helpers;
using Agent.Application.Interface;
using Agent.Domain.Entities.UserManagement;
using Agent.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Agent.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly  ITokenService _tokenService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthService(ITokenService tokenService, UserManager<ApplicationUser> userManager)
        {
            _tokenService = tokenService;
            _userManager = userManager;
        }

        public async Task<AuthTokenResponseDto>AuthTokenResponseAsync(ApplicationUser user)
        {
            
            var accessToken = await _tokenService.GenerateTokenAsync(user);
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id);
            var roles = await _userManager.GetRolesAsync(user);

            var data = new AuthTokenResponseDto
            {
                AccessToken = accessToken.AccessToken,
                RefreshToken = refreshToken,
                ExpiresIn = accessToken.ExpiresAt,
                User = new UserInfoDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    Role = roles.FirstOrDefault()
                }
            };
            return data;
        }
    }

}

