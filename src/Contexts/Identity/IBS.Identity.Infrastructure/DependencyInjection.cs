using IBS.Identity.Application.Queries;
using IBS.Identity.Application.Services;
using IBS.Identity.Domain.Repositories;
using IBS.Identity.Infrastructure.Persistence;
using IBS.Identity.Infrastructure.Queries;
using IBS.Identity.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IBS.Identity.Infrastructure;

/// <summary>
/// Extension methods for registering Identity Infrastructure services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Identity Infrastructure services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure JWT settings
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        // Register repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();

        // Register queries
        services.AddScoped<IUserQueries, UserQueries>();
        services.AddScoped<IRoleQueries, RoleQueries>();
        services.AddScoped<IPermissionQueries, PermissionQueries>();

        // Register services
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtService, JwtService>();

        return services;
    }
}
