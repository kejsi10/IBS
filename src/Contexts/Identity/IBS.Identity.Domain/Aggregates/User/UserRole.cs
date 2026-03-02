using IBS.BuildingBlocks.Domain;
using IBS.Identity.Domain.Aggregates.Role;

namespace IBS.Identity.Domain.Aggregates.User;

/// <summary>
/// Represents the association between a user and a role.
/// </summary>
public sealed class UserRole : Entity
{
    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Gets the role identifier.
    /// </summary>
    public Guid RoleId { get; private set; }

    /// <summary>
    /// Gets the role (navigation property for queries).
    /// </summary>
    public Role.Role? Role { get; private set; }

    /// <summary>
    /// Gets the date when this role was assigned.
    /// </summary>
    public DateTimeOffset AssignedAt { get; private set; }

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    private UserRole() { }

    /// <summary>
    /// Creates a new user-role association.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="roleId">The role identifier.</param>
    /// <returns>A new UserRole instance.</returns>
    internal static UserRole Create(Guid userId, Guid roleId)
    {
        return new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            AssignedAt = DateTimeOffset.UtcNow
        };
    }
}
