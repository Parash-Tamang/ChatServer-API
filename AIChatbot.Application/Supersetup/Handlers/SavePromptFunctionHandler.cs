using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Supersetup.Commands;
using AIChatbot.Application.Supersetup.DTOs;
using AIChatbot.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace AIChatbot.Application.Supersetup.Handlers
{
    public class SavePromptFunctionHandler
        : IRequestHandler<SavePromptFunctionCommand, List<FunctionDto>>
    {
        private readonly IPromptFunctionRepository _globalRepo;
        private readonly ILocalFunctionRepository _localRepo;
        private readonly IConnectionRepository _connectionRepo;

        public SavePromptFunctionHandler(
            IPromptFunctionRepository globalRepo,
            ILocalFunctionRepository localRepo,
            IConnectionRepository connectionRepo)
        {
            _globalRepo = globalRepo;
            _localRepo = localRepo;
            _connectionRepo = connectionRepo;
        }

        public async Task<List<FunctionDto>> Handle(
            SavePromptFunctionCommand request,
            CancellationToken ct)
        {
            // =============================
            // 🔒 VALIDATION
            // =============================
            if (string.IsNullOrWhiteSpace(request.FunctionName))
                throw new BadHttpRequestException("Function name cannot be empty.");

            if (string.IsNullOrWhiteSpace(request.SystemPrompt))
                throw new BadHttpRequestException("System prompt cannot be empty.");

            // ============================================================
            // 🔥 GLOBAL FUNCTIONS
            // ============================================================

            var globalFunctions = await _globalRepo.GetGlobalFunctionsAsync();

            var allowedNames = globalFunctions
                .Where(x => !x.IsDeleted)
                .Select(x => x.FunctionName)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // ============================================================
            // ✅ GLOBAL CREATE
            // ============================================================
            if (!request.ConnectionStringId.HasValue && !request.FunctionId.HasValue)
            {
                var newGlobal = new PromptFunction
                {
                    Id = Guid.NewGuid(),
                    ConnectionStringId = null,
                    FunctionName = request.FunctionName,
                    SystemPrompt = request.SystemPrompt,
                    IsDeleted = false
                };

                await _globalRepo.AddAsync(newGlobal);

                return await GetGlobalFunctions();
            }

            // ============================================================
            // ✅ GLOBAL UPDATE
            // ============================================================
            if (!request.ConnectionStringId.HasValue && request.FunctionId.HasValue)
            {
                var existing = await _globalRepo.GetByIdAsync(request.FunctionId.Value)
                    ?? throw new KeyNotFoundException("Global function not found.");

                if (existing.ConnectionStringId != null)
                    throw new BadHttpRequestException("This is not a global function.");

                existing.FunctionName = request.FunctionName;
                existing.SystemPrompt = request.SystemPrompt;

                await _globalRepo.UpdateAsync(existing);

                return await GetGlobalFunctions();
            }

            // ============================================================
            // 🔒 LOCAL VALIDATION
            // ============================================================

            var connectionId = request.ConnectionStringId!.Value;

            var connection = await _connectionRepo.GetByIdAsync(connectionId)
                ?? throw new KeyNotFoundException("Connection not found.");

            if (!allowedNames.Contains(request.FunctionName))
            {
                throw new BadHttpRequestException(
                    "Function name must exist in global functions.");
            }

            // ============================================================
            // 🔥 LOCAL OVERRIDE LOGIC (NEW SYSTEM)
            // ============================================================

            var existingBlock = await _localRepo.GetAsync(
                connectionId,
                request.FunctionName);

            var global = globalFunctions
                .FirstOrDefault(g => g.FunctionName.Equals(
                    request.FunctionName,
                    StringComparison.OrdinalIgnoreCase));

            if (existingBlock != null)
            {
                // 🔥 UPDATE OVERRIDE
                existingBlock.IsOverride = true;
                existingBlock.OverridePrompt = request.SystemPrompt;
      

                await _localRepo.UpdateAsync(existingBlock);
            }
            else
            {
                // 🔥 CREATE OVERRIDE
                var block = new LocalFunctionBlock
                {
                    Id = Guid.NewGuid(),
                    ConnectionId = connectionId,
                    FunctionName = request.FunctionName,
                    GlobalFunctionId = global!.Id,

                    IsOverride = true,
                    OverridePrompt = request.SystemPrompt,
                  

                    CreatedAt = DateTime.UtcNow
                };

                await _localRepo.AddRangeAsync(new List<LocalFunctionBlock> { block });
            }

            return await GetConnectionFunctions(connectionId);
        }

        // =============================
        private async Task<List<FunctionDto>> GetGlobalFunctions()
        {
            var functions = await _globalRepo.GetGlobalFunctionsAsync();

            return functions
                .Where(f => !f.IsDeleted)
                .Select(f => new FunctionDto
                {
                    Id = f.Id,
                    FunctionName = f.FunctionName,
                    SystemPrompt = f.SystemPrompt,
                    Source = "global"
                })
                .ToList();
        }

        // =============================
        private async Task<List<FunctionDto>> GetConnectionFunctions(Guid connectionId)
        {
            var blocks = await _localRepo.GetByConnectionAsync(connectionId);
            var globals = await _globalRepo.GetGlobalFunctionsAsync();

            return blocks.Select(block =>
            {
                var global = globals.FirstOrDefault(g => g.Id == block.GlobalFunctionId);

                return new FunctionDto
                {
                    Id = block.Id,
                    FunctionName = block.FunctionName,
                    SystemPrompt = block.IsOverride
                        ? block.OverridePrompt!
                        : global?.SystemPrompt!,
                    Source = block.IsOverride ? "local" : "global"
                };
            }).ToList();
        }
    }
}