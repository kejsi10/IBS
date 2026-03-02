using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Identity.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace IBS.Identity.Application.Commands.AssignRole;

/// <summary>
/// Handler for the AssignRoleCommand.
/// </summary>
public sealed class AssignRoleCommandHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork,
    ILogger<AssignRoleCommandHandler> logger) : ICommandHandler<AssignRoleCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        // Load user with roles
        var user = await userRepository.GetByIdWithRolesAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Error.NotFound("User", request.UserId);
        }

        // Verify role exists
        var role = await roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
        if (role is null)
        {
            return Error.NotFound("Role", request.RoleId);
        }

        // Assign role (domain handles idempotency)
        user.AssignRole(request.RoleId);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("[Audit] Role {RoleId} ({RoleName}) assigned to user {UserId}",
            request.RoleId, role.Name, request.UserId);
        return Result.Success();
    }
}
