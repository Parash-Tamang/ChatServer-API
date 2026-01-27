using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Application.Dto.UserManagement
{
     public class AuthTokenResponseDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; } // or cookie-only
        public DateTime ExpiresIn { get; set; }        // ACCESS TOKEN ONLY (seconds)
        public UserInfoDto User { get; set; }
    }
}
