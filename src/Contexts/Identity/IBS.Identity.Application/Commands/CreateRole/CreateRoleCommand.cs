using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Identity.Application.Commands.CreateRole;

/// <summary>
/// Command to create a new tenant-specific role.
/// </summary>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="Name">The role name.</param>
/// <param name="Description">The role description (optional).</param>
public sealed record CreateRoleCommand(
    Guid TenantId,
    string Name,
    string? Description = null) : ICommand<Guid>;
