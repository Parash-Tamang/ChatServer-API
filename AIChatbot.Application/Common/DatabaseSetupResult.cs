using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot.Application.Common
{
    public class DatabaseSetupResult
    {
        public Guid Id { get; set; }

        public bool DbStatus { get; set; }

        public bool SchemaCreated { get; set; }

        public bool ViewsCreated { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
