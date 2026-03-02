using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Identity.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace IBS.Identity.Application.Commands.RevokePermission;

/// <summary>
/// Handler for the RevokePermissionCommand.
/// </summary>
public sealed class RevokePermissionCommandHandler(
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork,
    ILogger<RevokePermissionCommandHandler> logger) : ICommandHandler<RevokePermissionCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(RevokePermissionCommand request, CancellationToken cancellationToken)
    {
        // Load role with permissions
        var role = await roleRepository.GetByIdWithPermissionsAsync(request.RoleId, cancellationToken);
        if (role is null)
        {
            return Error.NotFound("Role", request.RoleId);
        }

        // Revoke permission (domain handles idempotency)
        role.RevokePermission(request.PermissionId);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("[Audit] Permission {PermissionId} revoked from role {RoleId} ({RoleName})",
            request.PermissionId, request.RoleId, role.Name);
        return Result.Success();
    }
}
