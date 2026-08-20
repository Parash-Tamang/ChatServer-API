using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Common;
using AIChatbot.Application.RoleConnectionAccessRetrive.Commands;
using MediatR;

namespace AIChatbot.Application.RoleConnectionAccessRetrive.Handlers;

public class DeleteRuntimePermissionHandler
    : IRequestHandler<
        DeleteRuntimePermissionCommand,
        GenericResult>
{
    private readonly
        IRoleRuntimePermissionRepository
        _repo;

    private readonly
        IRuntimePermissionViewRepository
        _viewRepo;

    private readonly
        IRuntimeViewService
        _viewService;

    public DeleteRuntimePermissionHandler(
        IRoleRuntimePermissionRepository repo,

        IRuntimePermissionViewRepository viewRepo,

        IRuntimeViewService viewService)
    {
        _repo = repo;

        _viewRepo = viewRepo;

        _viewService = viewService;
    }

    public async Task<GenericResult> Handle(
        DeleteRuntimePermissionCommand request,
        CancellationToken cancellationToken)
    {
        // ============================================================
        // DELETE RUNTIME PERMISSIONS
        // ============================================================

        await _repo.DeleteAsync(
            request.RoleId,
            request.ConnectionId);

        // ============================================================
        // DELETE VIEW
        // ============================================================

        var existingView =
            await _viewRepo.GetAsync(
                request.RoleId,
                request.ConnectionId);

        if (existingView != null)
        {
            await _viewService.DeleteViewAsync(
                request.ConnectionId,
                existingView.ViewName);

            await _viewRepo.DeleteAsync(
                existingView);
        }

        return new GenericResult
        {
            Success = true,
            Message =
                "Runtime permissions deleted successfully"
        };
    }
}