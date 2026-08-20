
using AIChatbot.Application.Auth.Results;
using MediatR;

public record VerifyOtpCommand(string Email, string Otp)
    : IRequest<bool>;