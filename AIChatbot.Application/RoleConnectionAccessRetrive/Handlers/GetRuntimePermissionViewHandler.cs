using AIChatbot.Application.Abstractions;
using AIChatbot.Application.RoleConnectionAccessRetrive.DTOs;
using AIChatbot.Application.RoleConnectionAccessRetrive.Queries;
using MediatR;

namespace AIChatbot.Application.RoleConnectionAccessRetrive.Handlers;

public class GetRuntimePermissionViewHandler
    : IRequestHandler<
        GetRuntimePermissionViewQuery,
        RuntimePermissionViewDto?>
{
    private readonly
        IRuntimePermissionViewRepository
        _repo;

    private readonly
        IRoleConnectionUserLookupConfigurationRepository
        _lookupConfigRepo;

    public GetRuntimePermissionViewHandler(
        IRuntimePermissionViewRepository repo,
        IRoleConnectionUserLookupConfigurationRepository lookupConfigRepo)
    {
        _repo = repo;
        _lookupConfigRepo = lookupConfigRepo;
    }

    public async Task<RuntimePermissionViewDto?>
        Handle(
            GetRuntimePermissionViewQuery request,
            CancellationToken cancellationToken)
    {
        var data =
            await _repo.GetAsync(
                request.RoleId,
                request.ConnectionId);

        if (data == null)
        {
            return null;
        }

        var lookupConfig =
            await _lookupConfigRepo.GetAsync(
                request.RoleId,
                request.ConnectionId);

        return new RuntimePermissionViewDto
        {
            ViewName = data.ViewName,

            ViewSql = data.ViewSql,

            UserLookupConfiguration =
                lookupConfig != null
                    ? new UserLookupConfigurationDto
                    {
                        UserTableName = lookupConfig.UserTableName,
                        UserIdColumn = lookupConfig.UserIdColumn,
                        EmailColumn = lookupConfig.EmailColumn,
                        PhoneColumn = lookupConfig.PhoneColumn
                    }
                    : null
        };
    }
}
