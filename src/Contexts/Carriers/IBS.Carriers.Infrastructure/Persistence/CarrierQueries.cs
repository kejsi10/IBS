using IBS.Carriers.Domain.Aggregates.Carrier;
using IBS.Carriers.Domain.Queries;
using IBS.Carriers.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace IBS.Carriers.Infrastructure.Persistence;

/// <summary>
/// Query implementation for reading Carrier data with no-tracking.
/// </summary>
public sealed class CarrierQueries : ICarrierQueries
{
    private readonly DbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="CarrierQueries"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public CarrierQueries(DbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<Carrier?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Carrier>()
            .AsNoTracking()
            .Include(c => c.Products)
            .Include(c => c.Appetites)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Carrier?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();

        return await _context.Set<Carrier>()
            .AsNoTracking()
            .Include(c => c.Products)
            .Include(c => c.Appetites)
            .FirstOrDefaultAsync(c => c.Code.Value == normalizedCode, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Carrier>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<Carrier>()
            .AsNoTracking()
            .Include(c => c.Products)
            .Include(c => c.Appetites)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Carrier>> GetByStatusAsync(
        CarrierStatus status,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<Carrier>()
            .AsNoTracking()
            .Include(c => c.Products)
            .Include(c => c.Appetites)
            .Where(c => c.Status == status)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Carrier>> GetByLineOfBusinessAsync(
        LineOfBusiness lineOfBusiness,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<Carrier>()
            .AsNoTracking()
            .Include(c => c.Products)
            .Include(c => c.Appetites)
            .Where(c => c.Appetites.Any(a => a.LineOfBusiness == lineOfBusiness && a.IsActive))
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Carrier>> GetByStateAndLineOfBusinessAsync(
        LineOfBusiness lineOfBusiness,
        string state,
        CancellationToken cancellationToken = default)
    {
        var normalizedState = state.Trim().ToUpperInvariant();

        // Note: Complex state matching with "ALL" needs to be done in memory
        // or with a more sophisticated query approach
        var carriers = await _context.Set<Carrier>()
            .AsNoTracking()
            .Include(c => c.Products)
            .Include(c => c.Appetites)
            .Where(c => c.Status == CarrierStatus.Active)
            .Where(c => c.Appetites.Any(a =>
                a.LineOfBusiness == lineOfBusiness &&
                a.IsActive &&
                (a.States == "ALL" || a.States.Contains(normalizedState))))
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        // Filter more precisely in memory for comma-separated states
        return carriers
            .Where(c => c.CoversState(lineOfBusiness, state))
            .ToList();
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByCodeAsync(
        string code,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();

        var query = _context.Set<Carrier>()
            .AsNoTracking()
            .Where(c => c.Code.Value == normalizedCode);

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Carrier>> SearchByNameAsync(
        string searchTerm,
        CancellationToken cancellationToken = default)
    {
        var normalizedTerm = searchTerm.Trim().ToLowerInvariant();

        return await _context.Set<Carrier>()
            .AsNoTracking()
            .Include(c => c.Products)
            .Include(c => c.Appetites)
            .Where(c => c.Name.ToLower().Contains(normalizedTerm) ||
                       (c.LegalName != null && c.LegalName.ToLower().Contains(normalizedTerm)) ||
                       c.Code.Value.ToLower().Contains(normalizedTerm))
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }
}
