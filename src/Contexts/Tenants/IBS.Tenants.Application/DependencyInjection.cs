using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace IBS.Tenants.Application;

/// <summary>
/// Dependency injection extensions for the Tenants Application layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Tenants Application layer services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddTenantsApplication(this IServiceCollection services)
    {
        // Register validators from this assembly
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
