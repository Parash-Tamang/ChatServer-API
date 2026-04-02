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
        private readonly IPromptFunctionRepository _repo;
        private readonly IConnectionRepository _connectionRepo;

        public SavePromptFunctionHandler(
            IPromptFunctionRepository repo,
            IConnectionRepository connectionRepo)
        {
            _repo = repo;
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
            // ✅ CASE 4: CREATE GLOBAL FUNCTION (conn = null, func = null)
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

                await _repo.AddAsync(newGlobal);

                return await GetGlobalFunctions();
            }

            // ============================================================
            // ✅ CASE 1: UPDATE GLOBAL FUNCTION (conn = null, func = id)
            // ============================================================
            if (!request.ConnectionStringId.HasValue && request.FunctionId.HasValue)
            {
                var existing = await _repo.GetByIdAsync(request.FunctionId.Value);

                if (existing == null)
                    throw new KeyNotFoundException("Global function not found.");

                if (existing.ConnectionStringId != null)
                    throw new BadHttpRequestException("This function is not a global function.");

                existing.FunctionName = request.FunctionName;
                existing.SystemPrompt = request.SystemPrompt;

                await _repo.UpdateAsync(existing);

                return await GetGlobalFunctions();
            }

            // ============================================================
            // VALIDATE CONNECTION
            // ============================================================
            var connectionId = request.ConnectionStringId!.Value;

            var connection = await _connectionRepo.GetByIdAsync(connectionId);

            if (connection == null)
                throw new KeyNotFoundException("Connection not found.");

            // ============================================================
            // ✅ CASE 2: CREATE CONNECTION FUNCTION (conn = id, func = null)
            // ============================================================
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
            // ============================================================
            // ✅ CASE 3: UPDATE CONNECTION FUNCTION (conn = id, func = id)
            // ============================================================
            else
            {
                var existing = await _repo.GetByIdAsync(request.FunctionId.Value);

                if (existing == null)
                    throw new KeyNotFoundException("Function not found.");

                if (existing.ConnectionStringId != connectionId)
                    throw new BadHttpRequestException(
                        "This function does not belong to the provided connection.");

                existing.FunctionName = request.FunctionName;
                existing.SystemPrompt = request.SystemPrompt;

                await _repo.UpdateAsync(existing);
            }

            return await GetConnectionFunctions(connectionId);
        }

        // =============================
        // 🔧 HELPER: GLOBAL FUNCTIONS
        // =============================
        private async Task<List<FunctionDto>> GetGlobalFunctions()
        {
            var functions = await _repo.GetGlobalFunctionsAsync();

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

        // =============================
        // 🔧 HELPER: CONNECTION FUNCTIONS
        // =============================
        private async Task<List<FunctionDto>> GetConnectionFunctions(Guid connectionId)
        {
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