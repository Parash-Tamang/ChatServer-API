using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace AIChatbot.Application.Auth.Commands
{


    public record ResendRegisterOtpCommand(string Email)
        : IRequest<bool>;
}
