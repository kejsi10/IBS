using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Identity.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace IBS.Identity.Application.Commands.GrantPermission;

/// <summary>
/// Handler for the GrantPermissionCommand.
/// </summary>
public sealed class GrantPermissionCommandHandler(
    IRoleRepository roleRepository,
    IPermissionRepository permissionRepository,
    IUnitOfWork unitOfWork,
    ILogger<GrantPermissionCommandHandler> logger) : ICommandHandler<GrantPermissionCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(GrantPermissionCommand request, CancellationToken cancellationToken)
    {
        // Load role with permissions
        var role = await roleRepository.GetByIdWithPermissionsAsync(request.RoleId, cancellationToken);
        if (role is null)
        {
            return Error.NotFound("Role", request.RoleId);
        }

        // Verify permission exists
        var permission = await permissionRepository.GetByIdAsync(request.PermissionId, cancellationToken);
        if (permission is null)
        {
            return Error.NotFound("Permission", request.PermissionId);
        }

        // Grant permission (domain handles idempotency)
        role.GrantPermission(request.PermissionId);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("[Audit] Permission {PermissionId} ({PermissionName}) granted to role {RoleId} ({RoleName})",
            request.PermissionId, permission.Name, request.RoleId, role.Name);
        return Result.Success();
    }
}
