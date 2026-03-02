using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace IBS.Claims.Application;

/// <summary>
/// Dependency injection extensions for the Claims Application layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Claims Application layer services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddClaimsApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        return services;
    }
}
