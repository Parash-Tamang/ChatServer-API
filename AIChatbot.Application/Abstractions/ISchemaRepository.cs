using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot.Application.Abstractions;

public interface ISchemaRepository
{
    Task<Dictionary<string, List<string>>> GetSchemaAsync(Guid connectionId);
}