using IBS.Clients.Application.Queries;
using IBS.Clients.Domain.Repositories;
using IBS.Clients.Infrastructure.Persistence;
using IBS.Clients.Infrastructure.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace IBS.Clients.Infrastructure;

/// <summary>
/// Extension methods for registering Clients Infrastructure services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Clients Infrastructure services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddClientsInfrastructure(this IServiceCollection services)
    {
        // Register repository
        services.AddScoped<IClientRepository, ClientRepository>();

        // Register queries
        services.AddScoped<IClientQueries, ClientQueries>();

        return services;
    }
}
