using System.Text.Json;
using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Domain;
using IBS.BuildingBlocks.Infrastructure.Multitenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace IBS.Infrastructure.Persistence;

/// <summary>
/// EF Core interceptor that captures entity changes and writes audit log entries.
/// </summary>
public sealed class AuditInterceptor(
    ICurrentUserService currentUserService,
    ITenantContext tenantContext) : SaveChangesInterceptor
{
    private List<AuditLog>? _pendingAuditEntries;

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            _pendingAuditEntries = CreateAuditEntries(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <inheritdoc />
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (_pendingAuditEntries is { Count: > 0 } && eventData.Context is not null)
        {
            // EF Core calls AcceptAllChanges() AFTER SavedChangesAsync returns, so at this
            // point all previously saved entities (Conversation, messages, etc.) are still in
            // their Modified/Added states with a stale RowVersion original snapshot. The
            // secondary SaveChangesAsync below would therefore try to re-run UPDATE/INSERT
            // commands with the old RowVersion, yielding "expected 1 row, got 0" and a
            // DbUpdateConcurrencyException. Manually accepting changes first resets every
            // previously saved entity to Unchanged with the correct RowVersion origin value so
            // the secondary save only processes the new AuditLog entries.
            //
            // IMPORTANT: AcceptAllChanges must be called BEFORE AddRange so that only the
            // previously saved entities are reset. The new AuditLog entries must remain in
            // Added state so the secondary save actually persists them.
            eventData.Context.ChangeTracker.AcceptAllChanges();

            eventData.Context.Set<AuditLog>().AddRange(_pendingAuditEntries);
            _pendingAuditEntries = null;

            var autoDetect = eventData.Context.ChangeTracker.AutoDetectChangesEnabled;
            eventData.Context.ChangeTracker.AutoDetectChangesEnabled = false;
            try
            {
                await eventData.Context.SaveChangesAsync(cancellationToken);
            }
            finally
            {
                eventData.Context.ChangeTracker.AutoDetectChangesEnabled = autoDetect;
            }
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private List<AuditLog> CreateAuditEntries(DbContext context)
    {
        var auditEntries = new List<AuditLog>();
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            // Skip audit log entries themselves to avoid recursion
            if (entry.Entity is AuditLog)
                continue;

            // Only audit domain entities
            if (entry.Entity is not Entity)
                continue;

            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
                continue;

            var action = entry.State switch
            {
                EntityState.Added => "Create",
                EntityState.Modified => "Update",
                EntityState.Deleted => "Delete",
                _ => "Unknown"
            };

            var entityType = entry.Entity.GetType().Name;
            var entityId = entry.Property("Id").CurrentValue?.ToString() ?? "";

            string? changes = null;
            if (entry.State == EntityState.Modified)
            {
                var changedProps = new Dictionary<string, object?>();
                foreach (var prop in entry.Properties.Where(p => p.IsModified))
                {
                    changedProps[prop.Metadata.Name] = new
                    {
                        Old = prop.OriginalValue,
                        New = prop.CurrentValue
                    };
                }
                if (changedProps.Count > 0)
                {
                    changes = JsonSerializer.Serialize(changedProps);
                }
            }
            else if (entry.State == EntityState.Added)
            {
                var props = new Dictionary<string, object?>();
                foreach (var prop in entry.Properties)
                {
                    props[prop.Metadata.Name] = prop.CurrentValue;
                }
                changes = JsonSerializer.Serialize(props);
            }

            auditEntries.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                TenantId = tenantContext.HasTenant ? tenantContext.TenantId : null,
                UserId = currentUserService.IsAuthenticated ? currentUserService.UserId : null,
                UserEmail = null, // Could be populated if ICurrentUserService exposes email
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Changes = changes,
                Timestamp = now
            });
        }

        return auditEntries;
    }
}
