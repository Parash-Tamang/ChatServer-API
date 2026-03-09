using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Supersetup.Commands;
using AIChatbot.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AIChatbot.Application.Supersetup.Handlers
{
    public class DeleteSupersetupHandler
        : IRequestHandler<DeleteSupersetupCommand, bool>
    {
        private readonly IConnectionRepository _connectionRepo;
        private readonly IPromptFunctionRepository _functionRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public DeleteSupersetupHandler(
            IConnectionRepository connectionRepo,
            IPromptFunctionRepository functionRepo,
            UserManager<ApplicationUser> userManager)
        {
            _connectionRepo = connectionRepo;
            _functionRepo = functionRepo;
            _userManager = userManager;
        }

        public async Task<bool> Handle(
            DeleteSupersetupCommand request,
            CancellationToken ct)
        {


            // 🗑 Delete Function
            if (request.FunctionId != null )
            {
                var function = await _functionRepo.GetByIdAsync(request.FunctionId.Value)
                    ?? throw new Exception("Function not found");

                function.IsDeleted = true;
                await _functionRepo.UpdateAsync(function);

                return true;
            }

            // 🗑 Delete Connection
            if (request.ConnectionId != null)
            {
                var connection = await _connectionRepo.GetByIdAsync(request.ConnectionId.Value)
                    ?? throw new Exception("Connection not found");

                connection.IsDeleted = true;
                connection.IsActive = false;

                await _connectionRepo.UpdateAsync(connection);

                return true;
            }

            throw new Exception("Invalid delete request");
        }
    }
}