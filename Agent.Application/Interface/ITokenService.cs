using Agent.Application.Dto.UserManagement;
using Agent.Domain.Entities.UserManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Application.Interface
{
    public interface ITokenService
    {
        Task<string> GenerateRefreshTokenAsync(String userId); // generate refresh token
        Task<JwtTokenResponseDto> GenerateTokenAsync(ApplicationUser user); //generate access token
        Task<RefreshToken> ValidateAndConsumeRefreshTokenAsync(string refreshToken); // validate refresh token
        Task RevokeAllRefreshTokenAsync(string user); // for logout and password change

        Task RevokeAsync(string refreshToken);
    }
}

