using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Supersetup.Commands;
using AIChatbot.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace AIChatbot.Application.Supersetup.Handlers
{
    public class DeleteSupersetupHandler
        : IRequestHandler<DeleteSupersetupCommand, bool>
    {
        private readonly IConnectionRepository _connectionRepo;
        private readonly IPromptFunctionRepository _functionRepo;

        public DeleteSupersetupHandler(
            IConnectionRepository connectionRepo,
            IPromptFunctionRepository functionRepo)
        {
            _connectionRepo = connectionRepo;
            _functionRepo = functionRepo;
        }

        public async Task<bool> Handle(
            DeleteSupersetupCommand request,
            CancellationToken ct)
        {
            // =============================
            // ❌ INVALID REQUEST
            // =============================
            if (!request.ConnectionId.HasValue && !request.FunctionId.HasValue)
                throw new BadHttpRequestException(
                    "Either ConnectionId or FunctionId must be provided.");

            // ============================================================
            // ✅ CASE 1: DELETE FUNCTION (conn = id, func = id)
            // ============================================================
            if (request.ConnectionId.HasValue && request.FunctionId.HasValue)
            {
                var function = await _functionRepo.GetByIdAsync(request.FunctionId.Value);

                if (function == null)
                    throw new KeyNotFoundException("Function not found.");

                if (function.ConnectionStringId != request.ConnectionId.Value)
                    throw new BadHttpRequestException(
                        "Function does not belong to the provided connection.");

                function.IsDeleted = true;
                await _functionRepo.UpdateAsync(function);

                return true;
            }

            // ============================================================
            // ✅ CASE 3: DELETE GLOBAL FUNCTION (conn = null, func = id)
            // ============================================================
            if (!request.ConnectionId.HasValue && request.FunctionId.HasValue)
            {
                var function = await _functionRepo.GetByIdAsync(request.FunctionId.Value);

                if (function == null)
                    throw new KeyNotFoundException("Global function not found.");

                if (function.ConnectionStringId != null)
                    throw new BadHttpRequestException(
                        "This function is not a global function.");

                function.IsDeleted = true;
                await _functionRepo.UpdateAsync(function);

                return true;
            }

            // ============================================================
            // ✅ CASE 2: DELETE CONNECTION (conn = id, func = null)
            // ============================================================
            if (request.ConnectionId.HasValue && !request.FunctionId.HasValue)
            {
                var connection = await _connectionRepo.GetByIdAsync(request.ConnectionId.Value);

                if (connection == null)
                    throw new KeyNotFoundException("Connection not found.");

                connection.IsDeleted = true;
                connection.IsActive = false;

                await _connectionRepo.UpdateAsync(connection);

                return true;
            }

            // =============================
            // ❌ FALLBACK (SHOULD NOT HIT)
            // =============================
            throw new BadHttpRequestException("Invalid delete request.");
        }
    }
}