using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Supersetup.Commands;
using AIChatbot.Application.Supersetup.DTOs;
using AIChatbot.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

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
            // =========================================
            // 🔒 VALIDATION
            // =========================================

            if (string.IsNullOrWhiteSpace(request.FunctionName))
                throw new BadHttpRequestException(
                    "Function name cannot be empty.");

            if (string.IsNullOrWhiteSpace(request.SystemPrompt))
                throw new BadHttpRequestException(
                    "System prompt cannot be empty.");

            // =========================================
            // 🔥 DUPLICATE FUNCTION NAME CHECK
            // =========================================

            var existingName = await _repo.GetByNameAsync(request.FunctionName);

            if (existingName != null &&
                existingName.Id != request.FunctionId)
            {
                throw new BadHttpRequestException(
                    "Function name already exists.");
            }

            PromptFunction entity;

            // =========================================
            // ✏️ UPDATE EXISTING GLOBAL PROMPT
            // =========================================

            if (request.FunctionId != null)
            {
                entity = await _repo.GetByIdAsync(request.FunctionId.Value);

                if (entity == null)
                {
                    throw new KeyNotFoundException(
                        "Global prompt not found.");
                }

                // 🔒 Ensure global only
                if (entity.ConnectionStringId != null)
                {
                    throw new BadHttpRequestException(
                        "This is not a global function.");
                }

                entity.FunctionName = request.FunctionName;
                entity.SystemPrompt = request.SystemPrompt;

                await _repo.UpdateAsync(entity);
            }

            // =========================================
            // ➕ CREATE NEW GLOBAL PROMPT
            // =========================================

            else
            {
                entity = new PromptFunction
                {
                    Id = Guid.NewGuid(),

                    // 🔥 GLOBAL ONLY
                    ConnectionStringId = null,

                    FunctionName = request.FunctionName,
                    SystemPrompt = request.SystemPrompt,

                    IsDeleted = false
                };

                await _repo.AddAsync(entity);
            }

            // =========================================
            // ✅ RESPONSE
            // =========================================

            return new FunctionDto
            {
                Id = entity.Id,
                FunctionName = entity.FunctionName,
                SystemPrompt = entity.SystemPrompt,
                Source = "global"
            };
        }
    }
}