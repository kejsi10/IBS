using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Identity.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace IBS.Identity.Application.Commands.RemoveRole;

/// <summary>
/// Handler for the RemoveRoleCommand.
/// </summary>
public sealed class RemoveRoleCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ILogger<RemoveRoleCommandHandler> logger) : ICommandHandler<RemoveRoleCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(RemoveRoleCommand request, CancellationToken cancellationToken)
    {
        // Load user with roles
        var user = await userRepository.GetByIdWithRolesAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Error.NotFound("User", request.UserId);
        }

        // Remove role (domain handles idempotency)
        user.RemoveRole(request.RoleId);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("[Audit] Role {RoleId} removed from user {UserId}", request.RoleId, request.UserId);
        return Result.Success();
    }
}
