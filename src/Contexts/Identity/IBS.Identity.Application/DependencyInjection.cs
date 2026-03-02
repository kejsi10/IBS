using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace IBS.Identity.Application;

/// <summary>
/// Extension methods for registering Identity Application services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Identity Application services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddIdentityApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // Register validators
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
