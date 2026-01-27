using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Application.Dto.UserManagement
{
   
        public class JwtTokenResponseDto
        {
            public string AccessToken { get; set; }
            public DateTime ExpiresAt { get; set; }
    
    }
}
