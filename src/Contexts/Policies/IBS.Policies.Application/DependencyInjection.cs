using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace IBS.Policies.Application;

/// <summary>
/// Dependency injection extensions for the Policies Application layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Policies Application layer services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPoliciesApplication(this IServiceCollection services)
    {
        // Register validators from this assembly
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
