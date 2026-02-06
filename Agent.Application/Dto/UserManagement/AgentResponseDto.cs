using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Application.Dto.UserManagement
{

    public class AgentResponseDto
    {
        public bool success { get; set; }
        public string message { get; set; }

        public List<ColumnMetaDto> columns { get; set; }
        public List<List<object>> rows { get; set; }
        public int row_count { get; set; }
        public string sql_generated { get; set; }
        public bool was_reconstructed { get; set; }
        public DateTime timestamp { get; set; }
    }

}
