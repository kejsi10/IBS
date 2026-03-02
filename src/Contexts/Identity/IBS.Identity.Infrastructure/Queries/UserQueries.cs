using IBS.Identity.Application.Queries;
using IBS.Identity.Domain.Aggregates.User;
using IBS.Identity.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace IBS.Identity.Infrastructure.Queries;

/// <summary>
/// Read-only query implementation for User data.
/// All queries use AsNoTracking for optimal performance.
/// </summary>
public sealed class UserQueries : IUserQueries
{
    private readonly DbContext _context;
    private readonly DbSet<User> _users;

    /// <summary>
    /// Initializes a new instance of the UserQueries class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public UserQueries(DbContext context)
    {
        _context = context;
        _users = context.Set<User>();
    }

    /// <inheritdoc />
    public async Task<UserDetailsDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _users
            .AsNoTracking()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user is null)
            return null;

        return new UserDetailsDto
        {
            Id = user.Id,
            TenantId = user.TenantId,
            Email = user.Email.Value,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Title = user.Title,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.Status == UserStatus.Active,
            LastLoginAt = user.LastLoginAt,
            Roles = user.UserRoles.Select(ur => new UserRoleDto(ur.RoleId, ur.Role?.Name ?? string.Empty)).ToList(),
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    /// <inheritdoc />
    public async Task<PagedResult<UserListItemDto>> SearchAsync(
        Guid tenantId,
        string? searchTerm,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _users
            .AsNoTracking()
            .Where(u => u.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(u =>
                u.FirstName.ToLower().Contains(term) ||
                u.LastName.ToLower().Contains(term) ||
                u.Email.Value.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserListItemDto
            {
                Id = u.Id,
                TenantId = u.TenantId,
                Email = u.Email.Value,
                FirstName = u.FirstName,
                LastName = u.LastName,
                IsActive = u.Status == UserStatus.Active,
                LastLoginAt = u.LastLoginAt,
                Roles = u.UserRoles.Select(ur => ur.Role!.Name).ToList()
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<UserListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    /// <inheritdoc />
    public async Task<bool> EmailExistsAsync(
        Guid tenantId,
        string email,
        Guid? excludeUserId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToUpperInvariant();

        var query = _users
            .AsNoTracking()
            .Where(u => u.TenantId == tenantId && u.Email.NormalizedValue == normalizedEmail);

        if (excludeUserId.HasValue)
        {
            query = query.Where(u => u.Id != excludeUserId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}
