using IBS.BuildingBlocks.Infrastructure.Persistence;
using IBS.Carriers.Domain.Aggregates.Carrier;
using IBS.Carriers.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IBS.Carriers.Infrastructure.Persistence;

/// <summary>
/// Repository implementation for the Carrier aggregate.
/// </summary>
public sealed class CarrierRepository : Repository<Carrier, DbContext>, ICarrierRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CarrierRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public CarrierRepository(DbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public override async Task<Carrier?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Include child entities when loading the aggregate
        return await DbSet
            .Include(c => c.Products)
            .Include(c => c.Appetites)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }
}
