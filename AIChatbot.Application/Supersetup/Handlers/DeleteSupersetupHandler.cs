using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Supersetup.Commands;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AIChatbot.Application.Supersetup.Handlers
{
    public class DeleteSupersetupHandler
        : IRequestHandler<DeleteSupersetupCommand, bool>
    {
        private readonly IConnectionRepository _connectionRepo;
        private readonly IPromptFunctionRepository _globalRepo;
        private readonly ILocalFunctionRepository _localRepo;
        private readonly IAiProviderService _ai;
        private readonly ILogger<DeleteSupersetupHandler> _logger;

        public DeleteSupersetupHandler(
            IConnectionRepository connectionRepo,
            IPromptFunctionRepository globalRepo,
            ILocalFunctionRepository localRepo,
            IAiProviderService ai,
            ILogger<DeleteSupersetupHandler> logger)
        {
            _connectionRepo = connectionRepo;
            _globalRepo = globalRepo;
            _localRepo = localRepo;
            _ai = ai;
            _logger = logger;
        }

        public async Task<bool> Handle(
            DeleteSupersetupCommand request,
            CancellationToken ct)
        {
            if (!request.ConnectionId.HasValue && !request.FunctionId.HasValue)
                throw new BadHttpRequestException(
                    "Either ConnectionId or FunctionId must be provided.");

            // ============================================================
            // ✅ CASE 1: REMOVE LOCAL OVERRIDE
            // ============================================================
            if (request.ConnectionId.HasValue && request.FunctionId.HasValue)
            {
                var function = await _globalRepo.GetByIdAsync(request.FunctionId.Value)
                    ?? throw new KeyNotFoundException("Function not found.");

                var block = await _localRepo.GetAsync(
                    request.ConnectionId.Value,
                    function.FunctionName);

                if (block == null)
                    throw new KeyNotFoundException("Local function not found.");

                // 🔥 REMOVE OVERRIDE
                block.IsOverride = false;
                block.OverridePrompt = null;
                block.OverrideVersion = null;

                await _localRepo.UpdateAsync(block);

                return true;
            }

            // ============================================================
            // ✅ CASE 2: DELETE GLOBAL FUNCTION
            // ============================================================
            if (!request.ConnectionId.HasValue && request.FunctionId.HasValue)
            {
                var function = await _globalRepo.GetByIdAsync(request.FunctionId.Value)
                    ?? throw new KeyNotFoundException("Global function not found.");

                if (function.ConnectionStringId != null)
                    throw new BadHttpRequestException("Not a global function.");

                function.IsDeleted = true;
                await _globalRepo.UpdateAsync(function);

                // 🔥 CLEAN LOCAL BLOCKS
                await _localRepo.RemoveOverridesByFunctionName(function.FunctionName);

                return true;
            }

            // ============================================================
            // ✅ CASE 3: DELETE CONNECTION
            // ============================================================
            if (request.ConnectionId.HasValue && !request.FunctionId.HasValue)
            {
                var connection = await _connectionRepo.GetByIdAsync(request.ConnectionId.Value)
                    ?? throw new KeyNotFoundException("Connection not found.");

                bool pythonDeleted = true;

                if (connection.Verified)
                {
                    pythonDeleted = await _ai.NotifyConnectionDeletedAsync(connection.Id);
                }
                else
                {
                    _logger.LogInformation(
                        "Skipping Python delete. KB not created for {ConnectionId}",
                        connection.Id);
                }

                if (!pythonDeleted)
                    throw new ApplicationException("Delete operation failed.");

                // 🔥 DELETE LOCAL BLOCKS
                await _localRepo.DeleteByConnectionIdAsync(connection.Id);

                await _connectionRepo.DeleteAsync(connection.Id);

                return true;
            }

            throw new BadHttpRequestException("Invalid delete request.");
        }
    }
}