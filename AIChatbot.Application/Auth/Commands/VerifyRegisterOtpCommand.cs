using MediatR;

namespace AIChatbot.Application.Auth.Commands;

public record VerifyRegisterOtpCommand(string Email, string Otp)
    : IRequest<string>; // returns RegisterToken