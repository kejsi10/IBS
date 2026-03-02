using IBS.BuildingBlocks.Infrastructure.Multitenancy;
using IBS.Identity.Application.Commands.ForgotPassword;
using IBS.Identity.Application.Commands.Login;
using IBS.Identity.Application.Commands.Logout;
using IBS.Identity.Application.Commands.RegisterUser;
using IBS.Identity.Application.Commands.RefreshToken;
using IBS.Identity.Application.Commands.ResetPassword;
using IBS.Identity.Application.Queries.GetCurrentUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace IBS.Api.Controllers;

/// <summary>
/// Controller for authentication operations.
/// </summary>
public sealed class AuthController : ApiControllerBase
{
    private readonly IMediator _mediator;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<AuthController> _logger;

    /// <summary>
    /// Initializes a new instance of the AuthController class.
    /// </summary>
    /// <param name="mediator">The mediator.</param>
    /// <param name="tenantContext">The tenant context.</param>
    /// <param name="logger">The logger.</param>
    public AuthController(
        IMediator mediator,
        ITenantContext tenantContext,
        ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a user and returns tokens.
    /// </summary>
    /// <param name="request">The login request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Access and refresh tokens.</returns>
    /// <response code="200">Returns the authentication tokens.</response>
    /// <response code="400">If the tenant is not specified.</response>
    /// <response code="401">If the credentials are invalid.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        // Resolve tenant from header, context, or request body
        var tenantId = ResolveTenantId(request.TenantId);
        if (tenantId == Guid.Empty)
        {
            return BadRequest(new { error = "Tenant identifier is required. Provide X-Tenant-Id header or TenantId in request body." });
        }

        _logger.LogInformation("Login attempt for {Email} in tenant {TenantId}", request.Email, tenantId);

        var command = new LoginCommand(
            request.Email,
            request.Password,
            tenantId,
            GetClientIpAddress(),
            GetUserAgent());

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed login attempt for {Email} in tenant {TenantId}", request.Email, tenantId);
            return ToActionResult(result);
        }

        SetAuthCookies(result.Value.AccessToken, result.Value.RefreshToken, result.Value.ExpiresIn);

        return Ok(new LoginResponse
        {
            ExpiresIn = result.Value.ExpiresIn,
            UserId = result.Value.UserId,
            TenantId = result.Value.TenantId,
            Email = result.Value.Email,
            FullName = result.Value.FullName,
            Roles = result.Value.Roles.ToList()
        });
    }

    /// <summary>
    /// Refreshes an access token using a refresh token.
    /// </summary>
    /// <param name="request">The refresh token request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>New access and refresh tokens.</returns>
    /// <response code="200">Returns the new authentication tokens.</response>
    /// <response code="401">If the refresh token is invalid or expired.</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenRequest? request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Token refresh attempt");

        // Accept token from cookie (primary) or request body (backwards compat for API clients)
        var refreshToken = HttpContext.Request.Cookies["ibs_refresh_token"]
            ?? request?.RefreshToken;

        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(new { error = "Refresh token is required." });
        }

        var command = new RefreshTokenCommand(
            refreshToken,
            GetClientIpAddress(),
            GetUserAgent());

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return ToActionResult(result);
        }

        SetAuthCookies(result.Value.AccessToken, result.Value.RefreshToken, result.Value.ExpiresIn);

        return Ok(new LoginResponse
        {
            ExpiresIn = result.Value.ExpiresIn,
            UserId = result.Value.UserId,
            TenantId = result.Value.TenantId,
            Email = result.Value.Email,
            FullName = result.Value.FullName,
            Roles = result.Value.Roles.ToList()
        });
    }

    /// <summary>
    /// Logs out the current user by revoking refresh tokens.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content.</returns>
    /// <response code="204">If logout was successful.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Logout for user {UserId}", CurrentUserId);

        var command = new LogoutCommand(CurrentUserId);
        var result = await _mediator.Send(command, cancellationToken);

        ClearAuthCookies();
        return ToActionResult(result);
    }

    /// <summary>
    /// Initiates a password reset request.
    /// </summary>
    /// <param name="request">The password reset request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content (always, to prevent email enumeration).</returns>
    /// <response code="204">Always returned to prevent email enumeration.</response>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        // Resolve tenant from header, context, or request body
        var tenantId = ResolveTenantId(request.TenantId);
        if (tenantId == Guid.Empty)
        {
            // Return success to prevent tenant enumeration
            return NoContent();
        }

        _logger.LogInformation("Password reset requested for {Email} in tenant {TenantId}", request.Email, tenantId);

        var command = new ForgotPasswordCommand(request.Email, tenantId);

        // Execute but always return success to prevent email enumeration
        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Resets a password using a reset token.
    /// </summary>
    /// <param name="request">The password reset details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">If the password was reset successfully.</response>
    /// <response code="400">If the reset token is invalid or expired, or tenant is missing.</response>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        // Resolve tenant from header, context, or request body
        var tenantId = ResolveTenantId(request.TenantId);
        if (tenantId == Guid.Empty)
        {
            return BadRequest(new { error = "Tenant identifier is required." });
        }

        if (request.NewPassword != request.ConfirmPassword)
        {
            return BadRequest(new { error = "Passwords do not match." });
        }

        _logger.LogInformation("Password reset attempt for {Email} in tenant {TenantId}", request.Email, tenantId);

        var command = new ResetPasswordCommand(
            request.Token,
            request.Email,
            tenantId,
            request.NewPassword);

        var result = await _mediator.Send(command, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Gets the current user's profile.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The current user's profile.</returns>
    /// <response code="200">Returns the user's profile.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If the user is not found.</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(CurrentUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting current user {UserId}", CurrentUserId);

        var query = new GetCurrentUserQuery(CurrentUserId);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return ToActionResult(result);
        }

        return Ok(new CurrentUserResponse
        {
            Id = result.Value.Id,
            TenantId = result.Value.TenantId,
            Email = result.Value.Email,
            FirstName = result.Value.FirstName,
            LastName = result.Value.LastName,
            FullName = result.Value.FullName,
            Title = result.Value.Title,
            PhoneNumber = result.Value.PhoneNumber,
            IsActive = result.Value.IsActive,
            LastLoginAt = result.Value.LastLoginAt,
            Roles = result.Value.Roles.Select(r => r.Name).ToList()
        });
    }

    /// <summary>
    /// Registers a new user in the current tenant.
    /// </summary>
    /// <param name="request">The registration request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created user's identifier.</returns>
    /// <response code="201">Returns the newly created user ID.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="409">If the email already exists.</response>
    [HttpPost("register")]
    [Authorize]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Registering new user {Email} for tenant {TenantId}", request.Email, CurrentTenantId);

        var command = new RegisterUserCommand(
            CurrentTenantId,
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            request.Title,
            request.PhoneNumber);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            return StatusCode(StatusCodes.Status201Created, result.Value);
        }

        return ToActionResult(result);
    }

    /// <summary>
    /// Resolves the tenant ID from various sources.
    /// </summary>
    /// <param name="requestTenantId">The tenant ID from the request body.</param>
    /// <returns>The resolved tenant ID, or Guid.Empty if not found.</returns>
    private Guid ResolveTenantId(Guid? requestTenantId)
    {
        // Priority: 1. Request body, 2. Tenant context (from header/JWT)
        if (requestTenantId.HasValue && requestTenantId.Value != Guid.Empty)
        {
            return requestTenantId.Value;
        }

        if (_tenantContext.HasTenant)
        {
            return _tenantContext.TenantId;
        }

        return Guid.Empty;
    }

    /// <summary>
    /// Gets the client IP address. ForwardedHeaders middleware has already resolved
    /// the real client IP into <c>HttpContext.Connection.RemoteIpAddress</c>.
    /// </summary>
    /// <returns>The client IP address string.</returns>
    private string? GetClientIpAddress() =>
        HttpContext.Connection.RemoteIpAddress?.ToString();

    /// <summary>
    /// Sets the access and refresh token httpOnly cookies.
    /// </summary>
    /// <param name="accessToken">The JWT access token.</param>
    /// <param name="refreshToken">The refresh token.</param>
    /// <param name="accessExpiresInSeconds">Access token lifetime in seconds.</param>
    private void SetAuthCookies(string accessToken, string refreshToken, int accessExpiresInSeconds)
    {
        var isProduction = !HttpContext.RequestServices
            .GetRequiredService<IWebHostEnvironment>().IsDevelopment();

        var accessOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = isProduction,
            SameSite = SameSiteMode.Strict,
            MaxAge = TimeSpan.FromSeconds(accessExpiresInSeconds),
            Path = "/"
        };

        var refreshOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = isProduction,
            SameSite = SameSiteMode.Strict,
            MaxAge = TimeSpan.FromDays(7),
            Path = "/api/v1/auth/refresh"
        };

        HttpContext.Response.Cookies.Append("ibs_access_token", accessToken, accessOptions);
        HttpContext.Response.Cookies.Append("ibs_refresh_token", refreshToken, refreshOptions);
    }

    /// <summary>
    /// Clears the auth cookies on logout.
    /// </summary>
    private void ClearAuthCookies()
    {
        HttpContext.Response.Cookies.Delete("ibs_access_token");
        HttpContext.Response.Cookies.Delete("ibs_refresh_token");
    }

    /// <summary>
    /// Gets the user agent from the request.
    /// </summary>
    /// <returns>The user agent string.</returns>
    private string? GetUserAgent()
    {
        return HttpContext.Request.Headers.UserAgent.FirstOrDefault();
    }
}

/// <summary>
/// Login request model.
/// </summary>
public sealed record LoginRequest
{
    /// <summary>
    /// Gets the email address.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Gets the password.
    /// </summary>
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// Gets the tenant identifier (optional if X-Tenant-Id header is provided).
    /// </summary>
    public Guid? TenantId { get; init; }
}

/// <summary>
/// Login response model. Tokens are delivered as httpOnly cookies; this body carries
/// only the profile data the frontend needs to initialise its auth store.
/// </summary>
public sealed record LoginResponse
{
    /// <summary>
    /// Gets the access token expiration in seconds (for session-expiry UI).
    /// </summary>
    public int ExpiresIn { get; init; }

    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// Gets the tenant identifier.
    /// </summary>
    public Guid TenantId { get; init; }

    /// <summary>
    /// Gets the user's email.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Gets the user's full name.
    /// </summary>
    public string FullName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the user's roles.
    /// </summary>
    public List<string> Roles { get; init; } = [];
}

/// <summary>
/// Refresh token request model.
/// </summary>
public sealed record RefreshTokenRequest
{
    /// <summary>
    /// Gets the refresh token.
    /// </summary>
    public string RefreshToken { get; init; } = string.Empty;
}

/// <summary>
/// Forgot password request model.
/// </summary>
public sealed record ForgotPasswordRequest
{
    /// <summary>
    /// Gets the email address.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Gets the tenant identifier (optional if X-Tenant-Id header is provided).
    /// </summary>
    public Guid? TenantId { get; init; }
}

/// <summary>
/// Reset password request model.
/// </summary>
public sealed record ResetPasswordRequest
{
    /// <summary>
    /// Gets the reset token.
    /// </summary>
    public string Token { get; init; } = string.Empty;

    /// <summary>
    /// Gets the user's email address.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Gets the new password.
    /// </summary>
    public string NewPassword { get; init; } = string.Empty;

    /// <summary>
    /// Gets the new password confirmation.
    /// </summary>
    public string ConfirmPassword { get; init; } = string.Empty;

    /// <summary>
    /// Gets the tenant identifier (optional if X-Tenant-Id header is provided).
    /// </summary>
    public Guid? TenantId { get; init; }
}

/// <summary>
/// Current user response model.
/// </summary>
public sealed record CurrentUserResponse
{
    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the tenant identifier.
    /// </summary>
    public Guid TenantId { get; init; }

    /// <summary>
    /// Gets the email address.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Gets the first name.
    /// </summary>
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the last name.
    /// </summary>
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the full name.
    /// </summary>
    public string FullName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the title.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Gets the phone number.
    /// </summary>
    public string? PhoneNumber { get; init; }

    /// <summary>
    /// Gets whether the user is active.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Gets the last login timestamp.
    /// </summary>
    public DateTimeOffset? LastLoginAt { get; init; }

    /// <summary>
    /// Gets the user's roles.
    /// </summary>
    public List<string> Roles { get; init; } = [];
}

/// <summary>
/// Register user request model.
/// </summary>
public sealed record RegisterUserRequest
{
    /// <summary>
    /// Gets the email address.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Gets the password.
    /// </summary>
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// Gets the first name.
    /// </summary>
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the last name.
    /// </summary>
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the title (optional).
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Gets the phone number (optional).
    /// </summary>
    public string? PhoneNumber { get; init; }
}
