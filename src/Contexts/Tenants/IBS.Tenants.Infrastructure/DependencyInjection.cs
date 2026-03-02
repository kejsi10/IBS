using IBS.Tenants.Application.Queries;
using IBS.Tenants.Domain.Repositories;
using IBS.Tenants.Infrastructure.Persistence;
using IBS.Tenants.Infrastructure.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace IBS.Tenants.Infrastructure;

/// <summary>
/// Dependency injection extensions for the Tenants Infrastructure layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Tenants Infrastructure layer services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddTenantsInfrastructure(this IServiceCollection services)
    {
        // Register repository
        services.AddScoped<ITenantRepository, TenantRepository>();

        // Register queries
        services.AddScoped<ITenantQueries, TenantQueries>();

        return services;
    }
}
