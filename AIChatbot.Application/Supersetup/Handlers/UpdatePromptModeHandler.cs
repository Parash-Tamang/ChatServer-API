using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Supersetup.Commands;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace AIChatbot.Application.Supersetup.Handlers
{
    public class UpdatePromptModeHandler
        : IRequestHandler<UpdatePromptModeCommand, bool>
    {
        private readonly IConnectionRepository _connectionRepo;

        public UpdatePromptModeHandler(IConnectionRepository connectionRepo)
        {
            _connectionRepo = connectionRepo;
        }

        public async Task<bool> Handle(UpdatePromptModeCommand request, CancellationToken ct)
        {
            // 🔒 VALIDATION
            if (request.PromptingMode != 0 && request.PromptingMode != 1)
                throw new BadHttpRequestException("PromptingMode must be 0 (global) or 1 (local).");

            var connection = await _connectionRepo.GetByIdAsync(request.ConnectionId);

            if (connection == null)
                throw new KeyNotFoundException("Connection not found.");

            connection.PromptingMode = request.PromptingMode;

            await _connectionRepo.UpdateAsync(connection);

            return true;
        }
    }
}