using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Identity.Application.Queries;
using IBS.Identity.Domain.Aggregates.Role;
using IBS.Identity.Domain.Repositories;

namespace IBS.Identity.Application.Commands.CreateRole;

/// <summary>
/// Handler for the CreateRoleCommand.
/// </summary>
public sealed class CreateRoleCommandHandler(
    IRoleRepository roleRepository,
    IRoleQueries roleQueries,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateRoleCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        // Check name uniqueness within tenant
        var nameExists = await roleQueries.NameExistsAsync(request.TenantId, request.Name, cancellationToken: cancellationToken);
        if (nameExists)
        {
            return Error.Conflict($"A role with the name '{request.Name}' already exists.");
        }

        // Create role
        var role = Role.CreateTenantRole(request.TenantId, request.Name, request.Description);

        await roleRepository.AddAsync(role, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return role.Id;
    }
}
