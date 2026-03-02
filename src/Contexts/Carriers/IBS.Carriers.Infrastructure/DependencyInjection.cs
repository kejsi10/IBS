using IBS.Carriers.Domain.Queries;
using IBS.Carriers.Domain.Repositories;
using IBS.Carriers.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace IBS.Carriers.Infrastructure;

/// <summary>
/// Extension methods for registering Carrier Infrastructure services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Carrier Infrastructure services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddCarriersInfrastructure(this IServiceCollection services)
    {
        // Register repository
        services.AddScoped<ICarrierRepository, CarrierRepository>();

        // Register queries
        services.AddScoped<ICarrierQueries, CarrierQueries>();

        return services;
    }
}
