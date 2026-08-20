using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot.Application.Abstractions
{
    public interface ICaptchaService
    {
        string GenerateCaptcha(string ip);
        bool ValidateCaptcha(string ip, string input);
    }
}
