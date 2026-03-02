using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace IBS.Documents.Application;

/// <summary>
/// Dependency injection extensions for the Documents Application layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Documents Application layer services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDocumentsApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        return services;
    }
}
