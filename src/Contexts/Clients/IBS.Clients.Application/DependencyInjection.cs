using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace IBS.Clients.Application;

/// <summary>
/// Extension methods for registering Clients Application services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Clients Application services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddClientsApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // Register validators
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
