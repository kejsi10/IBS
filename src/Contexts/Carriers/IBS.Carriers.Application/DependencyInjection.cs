using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace IBS.Carriers.Application;

/// <summary>
/// Extension methods for registering Carrier Application services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Carrier Application services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddCarriersApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // Register validators
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
