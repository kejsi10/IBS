using System.Security.Claims;
using IBS.BuildingBlocks.Infrastructure.Multitenancy;

namespace IBS.Api.Middleware;

/// <summary>
/// Middleware for resolving the current tenant from the request.
/// </summary>
public sealed class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the TenantResolutionMiddleware class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger.</param>
    public TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="tenantContextAccessor">The tenant context accessor.</param>
    public async Task InvokeAsync(HttpContext context, ITenantContextAccessor tenantContextAccessor)
    {
        var tenantId = ResolveTenantId(context);

        if (tenantId.HasValue)
        {
            tenantContextAccessor.SetTenant(tenantId.Value);
            _logger.LogDebug("Tenant resolved: {TenantId}", tenantId.Value);
        }

        await _next(context);
    }

    private static Guid? ResolveTenantId(HttpContext context)
    {
        // First, try to get tenant from JWT claims
        var tenantClaim = context.User.FindFirst("tenant_id") ?? context.User.FindFirst("tid");
        if (tenantClaim != null && Guid.TryParse(tenantClaim.Value, out var tenantIdFromClaim))
        {
            return tenantIdFromClaim;
        }

        // Then, try to get from header (for API clients)
        if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantHeader) &&
            Guid.TryParse(tenantHeader.FirstOrDefault(), out var tenantIdFromHeader))
        {
            return tenantIdFromHeader;
        }

        // Finally, try to resolve from subdomain
        var host = context.Request.Host.Host;
        if (!string.IsNullOrEmpty(host) && host.Contains('.'))
        {
            var subdomain = host.Split('.')[0];
            // Note: In a real implementation, you would look up the tenant by subdomain
            // For now, we'll just skip subdomain resolution
        }

        return null;
    }
}

/// <summary>
/// Extension methods for TenantResolutionMiddleware.
/// </summary>
public static class TenantResolutionMiddlewareExtensions
{
    /// <summary>
    /// Adds tenant resolution middleware to the pipeline.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <returns>The application builder.</returns>
    public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TenantResolutionMiddleware>();
    }
}
