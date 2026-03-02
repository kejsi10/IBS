using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace IBS.PolicyAssistant.Application;

/// <summary>
/// Dependency injection extensions for the PolicyAssistant Application layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds PolicyAssistant Application services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPolicyAssistantApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        return services;
    }
}
