using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot.Application.Abstractions
{
    public interface IEmailService
    {
        Task SendResetLinkAsync(string email, string link);
        Task SendOtpAsync(string email, string otp);
    }
}
