using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace AIChatbot.Application.Common;

public class SqlFilterResult
{
    public string FilterSql { get; set; } = "";

    public Dictionary<string, object>
        Parameters
    { get; set; } = [];
}