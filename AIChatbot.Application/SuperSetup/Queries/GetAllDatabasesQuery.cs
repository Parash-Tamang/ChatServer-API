using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIChatbot.Domain.Entities;

using MediatR;

namespace AIChatbot.Application.Supersetup.Queries;

  public record GetAllDatabasesQuery(string RequestedByUserId)
        : IRequest<List<ConnectionString>>;

