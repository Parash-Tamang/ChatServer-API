using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Supersetup.Commands;
using AIChatbot.Application.Supersetup.DTOs;
using AIChatbot.Domain.Entities;
using MediatR;

namespace AIChatbot.Application.Supersetup.Handlers
{
    public class SaveGlobalPromptHandler
        : IRequestHandler<SaveGlobalPromptCommand, FunctionDto>
    {
        private readonly IPromptFunctionRepository _repo;

        public SaveGlobalPromptHandler(IPromptFunctionRepository repo)
        {
            _repo = repo;
        }

        public async Task<FunctionDto> Handle(
            SaveGlobalPromptCommand request,
            CancellationToken ct)
        {
            PromptFunction entity;

            // =========================
            // ✏️ UPDATE EXISTING PROMPT
            // =========================
            if (request.FunctionId != null)
            {
                entity = await _repo.GetByIdAsync(request.FunctionId.Value);

                if (entity == null)
                {
                    // ✅ Proper exception
                    throw new KeyNotFoundException("Global prompt not found.");
                }

                entity.FunctionName = request.FunctionName;
                entity.SystemPrompt = request.SystemPrompt;

                await _repo.UpdateAsync(entity);
            }

            // =========================
            // ➕ CREATE NEW PROMPT
            // =========================
            else
            {
                entity = new PromptFunction
                {
                    Id = Guid.NewGuid(),
                    ConnectionStringId = null, // Global prompt
                    FunctionName = request.FunctionName,
                    SystemPrompt = request.SystemPrompt,
                    IsDeleted = false
                };

                await _repo.AddAsync(entity);
            }

            // =========================
            // ✅ RETURN RESPONSE
            // =========================
            return new FunctionDto
            {
                Id = entity.Id,
                FunctionName = entity.FunctionName,
                SystemPrompt = entity.SystemPrompt
            };
        }
    }
}