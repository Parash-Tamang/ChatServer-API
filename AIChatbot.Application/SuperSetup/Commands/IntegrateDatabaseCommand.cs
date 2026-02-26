using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
  using MediatR;
    using AIChatbot.Application.DTOs;

namespace AIChatbot.Application.SuperSetup.Commands
{


    public record IntegrateDatabaseCommand(
        ConnectionTestDto Dto,
        string RequestedByUserId
    ) : IRequest<Guid>;
}