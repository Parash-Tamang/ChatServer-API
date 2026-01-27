using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agent.Application.Helpers;
using Agent.Domain.Entities.UserManagement;
using Agent.Application.Dto.UserManagement;
namespace Agent.Application.Interface
{
    public interface IAuthService
    {
        Task<AuthTokenResponseDto> AuthTokenResponseAsync(ApplicationUser user);

    }
}
