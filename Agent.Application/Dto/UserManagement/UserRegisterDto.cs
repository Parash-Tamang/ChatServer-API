using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Application.Dto.UserManagement
{
    public class UserRegisterDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }     
        public string Phone { get; set; }      // optional
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
