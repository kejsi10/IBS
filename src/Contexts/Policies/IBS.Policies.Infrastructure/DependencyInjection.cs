using IBS.Policies.Domain.Queries;
using IBS.Policies.Domain.Repositories;
using IBS.Policies.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace IBS.Policies.Infrastructure;

/// <summary>
/// Extension methods for registering Policies Infrastructure services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Policies Infrastructure services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddPoliciesInfrastructure(this IServiceCollection services)
    {
        // Register repositories
        services.AddScoped<IPolicyRepository, PolicyRepository>();
        services.AddScoped<IQuoteRepository, QuoteRepository>();

        // Register query services
        services.AddScoped<IQuoteQueries, QuoteQueries>();

        return services;
    }
}
