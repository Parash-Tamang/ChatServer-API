using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Common;

namespace AIChatbot.Infrastructure.Identity;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    // 🔐 SEND RESET LINK
    public async Task SendResetLinkAsync(string email, string link)
    {
        if (_settings.UseConsole)
        {
            Console.WriteLine("=================================");
            Console.WriteLine($"Reset link for {email}: {link}");
            Console.WriteLine("=================================");
            return;
        }

        using var smtp = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential(
                _settings.Email,
                _settings.AppPassword),
            EnableSsl = true
        };

        using var mail = new MailMessage
        {
            From = new MailAddress(_settings.Email),
            Subject = "Reset Your Password -- SUCHANA AI ",
            Body = $" Dear {email},\n\nClick the link below to reset your password:\n\n{link}\n\nIf you did not request a password reset, please ignore this email.\n\nJAI SIKKIM !!",
            IsBodyHtml = false
        };

        mail.To.Add(email);

        await smtp.SendMailAsync(mail);
    }

    // 🔐 SEND OTP
    public async Task SendOtpAsync(string email, string otp)
    {
        if (_settings.UseConsole)
        {
            Console.WriteLine("=================================");
            Console.WriteLine($"OTP for {email}: {otp}");
            Console.WriteLine("=================================");
            return;
        }

        using var smtp = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential(
                _settings.Email,
                _settings.AppPassword),
            EnableSsl = true
        };

        using var mail = new MailMessage
        {
            From = new MailAddress(_settings.Email),
            Subject = "OTP Code -- SUCHANA AI ",
            Body = $"Dear {email},\n\nYour OTP for SUCHANA AI is: {otp}\n\nThis OTP is valid for 3 minutes.\nDo not share this OTP with anyone. \n JAI SIKKIM !!",
            IsBodyHtml = false
        };

        mail.To.Add(email);

        await smtp.SendMailAsync(mail);
    }
}