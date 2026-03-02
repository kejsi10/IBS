using IBS.Commissions.Domain.Aggregates.CommissionSchedule;
using IBS.Commissions.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IBS.Commissions.Infrastructure.Persistence;

/// <summary>
/// Repository implementation for the CommissionSchedule aggregate.
/// </summary>
public sealed class CommissionScheduleRepository : ICommissionScheduleRepository
{
    private readonly DbContext _context;
    private readonly DbSet<CommissionSchedule> _schedules;

    /// <summary>
    /// Initializes a new instance of the CommissionScheduleRepository class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public CommissionScheduleRepository(DbContext context)
    {
        _context = context;
        _schedules = context.Set<CommissionSchedule>();
    }

    /// <inheritdoc />
    public async Task<CommissionSchedule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _schedules.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(CommissionSchedule schedule, CancellationToken cancellationToken = default)
    {
        await _schedules.AddAsync(schedule, cancellationToken);
    }

    /// <inheritdoc />
    public Task UpdateAsync(CommissionSchedule schedule, CancellationToken cancellationToken = default)
    {
        // Entity is already tracked by EF change tracker via GetByIdAsync.
        return Task.CompletedTask;
    }
}
