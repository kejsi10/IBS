using IBS.BuildingBlocks.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace IBS.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Unit of Work implementation using Entity Framework Core.
/// </summary>
/// <typeparam name="TContext">The type of the database context.</typeparam>
public class UnitOfWork<TContext> : IUnitOfWork
    where TContext : DbContext
{
    private readonly TContext _context;
    private IDbContextTransaction? _transaction;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWork{TContext}"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public UnitOfWork(TContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> SaveChangesWithConcurrencyRetryAsync(int maxRetries = 3, CancellationToken cancellationToken = default)
    {
        for (var attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (attempt == maxRetries - 1)
                    throw;

                foreach (var entry in ex.Entries)
                {
                    // Skip Added entries — they were never persisted and don't need
                    // conflict resolution. They appear in ex.Entries when a batch
                    // rollback prevents reading OUTPUT INSERTED values.
                    if (entry.State == EntityState.Added)
                        continue;

                    var dbValues = await entry.GetDatabaseValuesAsync(cancellationToken);
                    if (dbValues is null)
                        throw; // Entity was deleted

                    // Accept database values so the next attempt uses the current RowVersion
                    entry.OriginalValues.SetValues(dbValues);
                }
            }
        }

        return 0; // Unreachable
    }

    /// <inheritdoc />
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
            return;

        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
            throw new InvalidOperationException("No transaction has been started.");

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    /// <inheritdoc />
    public async Task RollbackTransactionAsync()
    {
        if (_transaction is null)
            return;

        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes resources.
    /// </summary>
    /// <param name="disposing">Whether to dispose managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _transaction?.Dispose();
            _context.Dispose();
        }

        _disposed = true;
    }
}
