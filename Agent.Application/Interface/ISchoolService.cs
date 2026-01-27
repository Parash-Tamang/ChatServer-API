using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Application.Interface
{
    public interface ISchoolService
    {
        Task<String> ExecuteQueryAsync(string sql);
    }
}
