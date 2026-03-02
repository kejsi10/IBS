using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Identity.Application.Queries;
using IBS.Identity.Domain.Repositories;

namespace IBS.Identity.Application.Commands.UpdateRole;

/// <summary>
/// Handler for the UpdateRoleCommand.
/// </summary>
public sealed class UpdateRoleCommandHandler(
    IRoleRepository roleRepository,
    IRoleQueries roleQueries,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateRoleCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        // Load role
        var role = await roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
        if (role is null)
        {
            return Error.NotFound("Role", request.RoleId);
        }

        // System roles cannot be modified
        if (role.IsSystemRole)
        {
            return Error.Validation("System roles cannot be modified.");
        }

        // Check name uniqueness (exclude self)
        var nameExists = await roleQueries.NameExistsAsync(
            request.TenantId, request.Name, request.RoleId, cancellationToken);
        if (nameExists)
        {
            return Error.Conflict($"A role with the name '{request.Name}' already exists.");
        }

        // Update role
        role.Update(request.Name, request.Description);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
