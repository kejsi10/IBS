using IBS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IBS.Api.Controllers;

/// <summary>
/// Controller for querying audit log entries.
/// </summary>
[Authorize]
public sealed class AuditController : ApiControllerBase
{
    private readonly IbsDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditController"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    public AuditController(IbsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Gets a paginated list of audit log entries with optional filtering.
    /// </summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="entityType">Filter by entity type.</param>
    /// <param name="entityId">Filter by entity ID.</param>
    /// <param name="action">Filter by action (Create, Update, Delete).</param>
    /// <param name="from">Filter from date.</param>
    /// <param name="to">Filter to date.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated list of audit log entries.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(AuditPagedResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? entityType = null,
        [FromQuery] string? entityId = null,
        [FromQuery] string? action = null,
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null,
        CancellationToken cancellationToken = default)
    {
        var tenantId = CurrentTenantId;
        var query = _dbContext.AuditLogs
            .AsNoTracking()
            .Where(a => a.TenantId == tenantId || a.TenantId == null);

        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(a => a.EntityType == entityType);

        if (!string.IsNullOrWhiteSpace(entityId))
            query = query.Where(a => a.EntityId == entityId);

        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(a => a.Action == action);

        if (from.HasValue)
            query = query.Where(a => a.Timestamp >= from.Value);

        if (to.HasValue)
            query = query.Where(a => a.Timestamp <= to.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                UserId = a.UserId,
                UserEmail = a.UserEmail,
                Action = a.Action,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                Changes = a.Changes,
                Timestamp = a.Timestamp
            })
            .ToListAsync(cancellationToken);

        return Ok(new AuditPagedResult
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }
}

/// <summary>
/// Audit log entry DTO for API responses.
/// </summary>
public sealed class AuditLogDto
{
    /// <summary>
    /// Gets or sets the audit log entry identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Gets or sets the user email.
    /// </summary>
    public string? UserEmail { get; set; }

    /// <summary>
    /// Gets or sets the action performed.
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the entity type.
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the entity identifier.
    /// </summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the JSON diff of changes.
    /// </summary>
    public string? Changes { get; set; }

    /// <summary>
    /// Gets or sets the timestamp.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }
}

/// <summary>
/// Paginated result for audit log entries.
/// </summary>
public sealed class AuditPagedResult
{
    /// <summary>
    /// Gets or sets the audit log entries.
    /// </summary>
    public IReadOnlyList<AuditLogDto> Items { get; set; } = [];

    /// <summary>
    /// Gets or sets the current page.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Gets or sets the page size.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Gets or sets the total count.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
}
