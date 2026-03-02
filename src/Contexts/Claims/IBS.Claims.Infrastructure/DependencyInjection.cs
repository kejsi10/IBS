using IBS.Claims.Domain.Queries;
using IBS.Claims.Domain.Repositories;
using IBS.Claims.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace IBS.Claims.Infrastructure;

/// <summary>
/// Extension methods for registering Claims Infrastructure services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Claims Infrastructure services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddClaimsInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IClaimRepository, ClaimRepository>();
        services.AddScoped<IClaimQueries, ClaimQueries>();

        return services;
    }
}
