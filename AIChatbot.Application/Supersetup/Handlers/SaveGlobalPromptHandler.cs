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

            // EDIT existing global prompt
            if (request.FunctionId != null)
            {
                entity = await _repo.GetByIdAsync(request.FunctionId.Value)
                         ?? throw new Exception("Global prompt not found");

                entity.FunctionName = request.FunctionName;
                entity.SystemPrompt = request.SystemPrompt;

                await _repo.UpdateAsync(entity);
            }

            // CREATE new global prompt
            else
            {
                entity = new PromptFunction
                {
                    Id = Guid.NewGuid(),
                    ConnectionStringId = null, // Important: global prompts have no connection
                    FunctionName = request.FunctionName,
                    SystemPrompt = request.SystemPrompt,
                    IsDeleted = false
                };

                await _repo.AddAsync(entity);
            }

            return new FunctionDto
            {
                Id = entity.Id,
                FunctionName = entity.FunctionName,
                SystemPrompt = entity.SystemPrompt
            };
        }
    }
}