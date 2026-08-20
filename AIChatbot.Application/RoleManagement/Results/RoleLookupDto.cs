using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot.Application.RoleManagement.Results
{
    public sealed class RoleLookupDto
    {
        public string RoleId { get; set; } = default!;

        public string RoleName { get; set; } = default!;
    }
}
