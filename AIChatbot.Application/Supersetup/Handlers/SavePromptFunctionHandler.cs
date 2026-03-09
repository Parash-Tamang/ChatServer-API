using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Supersetup.Commands;
using AIChatbot.Application.Supersetup.DTOs;
using AIChatbot.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AIChatbot.Application.Supersetup.Handlers
{
    public class SavePromptFunctionHandler
        : IRequestHandler<SavePromptFunctionCommand, List<FunctionDto>>
    {
        private readonly IPromptFunctionRepository _repo;
        private readonly IConnectionRepository _connectionRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public SavePromptFunctionHandler(
            IPromptFunctionRepository repo,
            IConnectionRepository connectionRepo,
            UserManager<ApplicationUser> userManager)
        {
            _repo = repo;
            _connectionRepo = connectionRepo;
            _userManager = userManager;
        }

        public async Task<List<FunctionDto>> Handle(
         SavePromptFunctionCommand request,
         CancellationToken ct)
        {
            // 🔐 SuperAdmin only


            // ✅ Validate ConnectionStringId FIRST
            if (!request.ConnectionStringId.HasValue)
                throw new Exception("ConnectionStringId is required");

            var connectionId = request.ConnectionStringId.Value;

            // ✅ Validate connection exists
            var connection = await _connectionRepo.GetByIdAsync(connectionId);
            if (connection == null)
                throw new Exception("Connection not found");

            // =============================
            // CREATE
            // =============================
            if (!request.FunctionId.HasValue)
            {
                var newFunction = new PromptFunction
                {
                    Id = Guid.NewGuid(),
                    ConnectionStringId = connectionId,
                    FunctionName = request.FunctionName,
                    SystemPrompt = request.SystemPrompt,
                    IsDeleted = false
                };

                await _repo.AddAsync(newFunction);
            }
            // =============================
            // UPDATE
            // =============================
            else
            {
                var existing = await _repo.GetByIdAsync(request.FunctionId.Value)
                    ?? throw new Exception("Function not found");

                existing.FunctionName = request.FunctionName;
                existing.SystemPrompt = request.SystemPrompt;

                await _repo.UpdateAsync(existing);
            }

            // =============================
            // RETURN UPDATED LIST
            // =============================
            var functions = await _repo.GetByConnectionIdAsync(connectionId);

            return functions
                .Where(f => !f.IsDeleted)
                .Select(f => new FunctionDto
                {
                    Id = f.Id,
                    FunctionName = f.FunctionName,
                    SystemPrompt = f.SystemPrompt
                })
                .ToList();
        }
    }
}