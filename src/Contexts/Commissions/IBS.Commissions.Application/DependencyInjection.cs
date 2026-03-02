using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace IBS.Commissions.Application;

/// <summary>
/// Dependency injection extensions for the Commissions Application layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Commissions Application layer services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCommissionsApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        return services;
    }
}
