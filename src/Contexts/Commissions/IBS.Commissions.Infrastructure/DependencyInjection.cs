using IBS.Commissions.Domain.Queries;
using IBS.Commissions.Domain.Repositories;
using IBS.Commissions.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace IBS.Commissions.Infrastructure;

/// <summary>
/// Extension methods for registering Commissions Infrastructure services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Commissions Infrastructure services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddCommissionsInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<ICommissionScheduleRepository, CommissionScheduleRepository>();
        services.AddScoped<ICommissionStatementRepository, CommissionStatementRepository>();
        services.AddScoped<ICommissionScheduleQueries, CommissionScheduleQueries>();
        services.AddScoped<ICommissionStatementQueries, CommissionStatementQueries>();

        return services;
    }
}
