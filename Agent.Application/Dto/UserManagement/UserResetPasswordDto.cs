using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Application.Dto.UserManagement
{
    public class UserResetPasswordDto
    {

        public string Email { get; set; }
        public string Token { get; set; }
        public string Password { get; set; }
    }
}
