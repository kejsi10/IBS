using IBS.Commissions.Domain.Aggregates.CommissionSchedule;
using IBS.Commissions.Domain.Queries;
using Microsoft.EntityFrameworkCore;

namespace IBS.Commissions.Infrastructure.Persistence;

/// <summary>
/// Read-side query implementation for commission schedules.
/// </summary>
public sealed class CommissionScheduleQueries : ICommissionScheduleQueries
{
    private readonly DbSet<CommissionSchedule> _schedules;

    /// <summary>
    /// Initializes a new instance of the CommissionScheduleQueries class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public CommissionScheduleQueries(DbContext context)
    {
        _schedules = context.Set<CommissionSchedule>();
    }

    /// <inheritdoc />
    public async Task<CommissionScheduleReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var schedule = await _schedules
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (schedule is null)
            return null;

        return MapToReadModel(schedule);
    }

    /// <inheritdoc />
    public async Task<ScheduleSearchResult> SearchAsync(ScheduleSearchFilter filter, CancellationToken cancellationToken = default)
    {
        var query = _schedules.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim().ToLower();
            query = query.Where(s => s.CarrierName.ToLower().Contains(term) ||
                                     s.LineOfBusiness.ToLower().Contains(term));
        }

        if (filter.CarrierId.HasValue)
            query = query.Where(s => s.CarrierId == filter.CarrierId.Value);

        if (!string.IsNullOrWhiteSpace(filter.LineOfBusiness))
            query = query.Where(s => s.LineOfBusiness == filter.LineOfBusiness);

        if (filter.IsActive.HasValue)
            query = query.Where(s => s.IsActive == filter.IsActive.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        query = filter.SortBy?.ToLower() switch
        {
            "carriername" => filter.SortDirection?.ToLower() == "asc"
                ? query.OrderBy(s => s.CarrierName)
                : query.OrderByDescending(s => s.CarrierName),
            "lineofbusiness" => filter.SortDirection?.ToLower() == "asc"
                ? query.OrderBy(s => s.LineOfBusiness)
                : query.OrderByDescending(s => s.LineOfBusiness),
            "newbusinessrate" => filter.SortDirection?.ToLower() == "asc"
                ? query.OrderBy(s => s.NewBusinessRate)
                : query.OrderByDescending(s => s.NewBusinessRate),
            "effectivefrom" => filter.SortDirection?.ToLower() == "asc"
                ? query.OrderBy(s => s.EffectiveFrom)
                : query.OrderByDescending(s => s.EffectiveFrom),
            _ => filter.SortDirection?.ToLower() == "asc"
                ? query.OrderBy(s => s.CreatedAt)
                : query.OrderByDescending(s => s.CreatedAt)
        };

        var schedules = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return new ScheduleSearchResult
        {
            Schedules = schedules.Select(MapToReadModel).ToList(),
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };
    }

    private static CommissionScheduleReadModel MapToReadModel(CommissionSchedule schedule)
    {
        return new CommissionScheduleReadModel(
            schedule.Id,
            schedule.CarrierId,
            schedule.CarrierName,
            schedule.LineOfBusiness,
            schedule.NewBusinessRate,
            schedule.RenewalRate,
            schedule.EffectiveFrom,
            schedule.EffectiveTo,
            schedule.IsActive,
            schedule.CreatedAt,
            schedule.UpdatedAt
        );
    }
}
