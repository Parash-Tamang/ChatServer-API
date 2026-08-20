using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot.Application.Supersetup.DTO_s
{
    public sealed class ConnectionLookupDto
    {
        public Guid Id { get; set; }

        public string ServerName { get; set; } = "";

        public string DatabaseName { get; set; } = "";

        public string AuthMode { get; set; } = "";
    }
}
