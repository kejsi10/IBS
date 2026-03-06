using System.Text;
using System.Threading.RateLimiting;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using IBS.Api.Configuration;
using IBS.Api.Middleware;
using IBS.Api.Services;
using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Behaviors;
using IBS.BuildingBlocks.Domain;
using IBS.BuildingBlocks.Infrastructure.Multitenancy;
using IBS.BuildingBlocks.Infrastructure.Persistence;
using IBS.Carriers.Application;
using IBS.Carriers.Infrastructure;
using IBS.Clients.Application;
using IBS.Clients.Infrastructure;
using IBS.Identity.Application;
using IBS.Identity.Infrastructure;
using IBS.Infrastructure.Persistence;
using IBS.Infrastructure.Services;
using IBS.Claims.Application;
using IBS.Claims.Infrastructure;
using IBS.Commissions.Application;
using IBS.Commissions.Infrastructure;
using IBS.Documents.Application;
using IBS.Documents.Infrastructure;
using IBS.Policies.Application;
using IBS.Policies.Infrastructure;
using IBS.PolicyAssistant.Application;
using IBS.PolicyAssistant.Infrastructure;
using IBS.Tenants.Application;
using IBS.Tenants.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting IBS API");

    var builder = WebApplication.CreateBuilder(args);

    // Add Application Insights (reads APPLICATIONINSIGHTS_CONNECTION_STRING env var automatically)
    builder.Services.AddApplicationInsightsTelemetry();
    builder.Services.AddApplicationInsightsTelemetryProcessor<HealthCheckTelemetryFilter>();

    // Configure Serilog — also sink to Application Insights so structured logs appear in AI
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.ApplicationInsights(
            services.GetRequiredService<Microsoft.ApplicationInsights.TelemetryClient>(),
            TelemetryConverter.Traces));

    // Add configuration
    var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
        ?? new JwtSettings();
    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));

    // Validate JWT secret key at startup — prevents insecure defaults from reaching production
    const string defaultPlaceholder = "DefaultSecretKeyForDevelopmentOnly!";
    if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey)
        || jwtSettings.SecretKey == defaultPlaceholder
        || Encoding.UTF8.GetByteCount(jwtSettings.SecretKey) < 32)
    {
        if (!builder.Environment.IsDevelopment())
        {
            throw new InvalidOperationException(
                "JWT SecretKey is missing, uses the default placeholder, or is shorter than 32 bytes. " +
                "Set a strong secret in configuration before running in non-Development environments.");
        }
        Log.Warning("JWT SecretKey uses the development placeholder. Set a strong key for production.");
    }

    // Add HttpContextAccessor
    builder.Services.AddHttpContextAccessor();

    // Add multi-tenancy
    builder.Services.AddScoped<ITenantContextAccessor, TenantContextAccessor>();
    builder.Services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<ITenantContextAccessor>());

    // Add current user service
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

    // Add email service
    builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(EmailSettings.SectionName));
    var emailSettings = builder.Configuration.GetSection(EmailSettings.SectionName).Get<EmailSettings>();
    if (emailSettings?.UseConsole == true || builder.Environment.IsDevelopment())
    {
        builder.Services.AddSingleton<IEmailService, ConsoleEmailService>();
    }
    else
    {
        builder.Services.AddSingleton<IEmailService, SmtpEmailService>();
    }

    // Add Redis distributed cache
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration.GetValue<string>("Redis:ConnectionString") ?? "localhost:6379";
        options.InstanceName = "IBS:";
    });
    builder.Services.AddSingleton<ICacheService, RedisCacheService>();

    // Add currency service
    builder.Services.AddSingleton<ICurrencyService, StaticCurrencyService>();

    // Add audit interceptor
    builder.Services.AddScoped<AuditInterceptor>();

    // Add Entity Framework Core with SQL Server
    builder.Services.AddDbContext<IbsDbContext>((sp, options) =>
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.MigrationsAssembly(typeof(IbsDbContext).Assembly.FullName);
            sqlOptions.EnableRetryOnFailure(3);
            sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        });
        options.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
    });

    // Register DbContext as the base type for repositories
    builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<IbsDbContext>());

    // Add Unit of Work
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork<IbsDbContext>>();

    // Add MediatR
    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(typeof(IBS.Clients.Application.DependencyInjection).Assembly);
        cfg.RegisterServicesFromAssembly(typeof(IBS.Identity.Application.DependencyInjection).Assembly);
        cfg.RegisterServicesFromAssembly(typeof(IBS.Carriers.Application.DependencyInjection).Assembly);
        cfg.RegisterServicesFromAssembly(typeof(IBS.Tenants.Application.DependencyInjection).Assembly);
        cfg.RegisterServicesFromAssembly(typeof(IBS.Policies.Application.DependencyInjection).Assembly);
        cfg.RegisterServicesFromAssembly(typeof(IBS.Claims.Application.DependencyInjection).Assembly);
        cfg.RegisterServicesFromAssembly(typeof(IBS.Commissions.Application.DependencyInjection).Assembly);
        cfg.RegisterServicesFromAssembly(typeof(IBS.Documents.Application.DependencyInjection).Assembly);
        cfg.RegisterServicesFromAssembly(typeof(IBS.PolicyAssistant.Application.DependencyInjection).Assembly);
        cfg.RegisterServicesFromAssembly(typeof(IBS.PolicyAssistant.Infrastructure.DependencyInjection).Assembly);

        // Add pipeline behaviors
        cfg.AddBehavior(typeof(MediatR.IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        cfg.AddBehavior(typeof(MediatR.IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    });

    // Add Application layer services
    builder.Services.AddClientsApplication();
    builder.Services.AddIdentityApplication();
    builder.Services.AddCarriersApplication();
    builder.Services.AddTenantsApplication();
    builder.Services.AddPoliciesApplication();
    builder.Services.AddClaimsApplication();
    builder.Services.AddCommissionsApplication();
    builder.Services.AddDocumentsApplication();
    builder.Services.AddPolicyAssistantApplication();

    // Add Infrastructure layer services
    builder.Services.AddClientsInfrastructure();
    builder.Services.AddIdentityInfrastructure(builder.Configuration);
    builder.Services.AddCarriersInfrastructure();
    builder.Services.AddTenantsInfrastructure();
    builder.Services.AddPoliciesInfrastructure();
    builder.Services.AddClaimsInfrastructure();
    builder.Services.AddCommissionsInfrastructure();
    builder.Services.AddDocumentsInfrastructure(builder.Configuration);
    builder.Services.AddPolicyAssistantInfrastructure(builder.Configuration);

    // Add authentication
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            // Zero clock skew: tokens expire at exactly the stated time
            ClockSkew = TimeSpan.Zero
        };

        // Read the JWT from the httpOnly cookie when no Authorization header is present
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                if (string.IsNullOrEmpty(ctx.Token))
                    ctx.Token = ctx.Request.Cookies["ibs_access_token"];
                return Task.CompletedTask;
            }
        };
    });

    builder.Services.AddAuthorization();

    // Add controllers
    builder.Services.AddControllers(options =>
        {
            options.Filters.Add<IBS.Api.Filters.ConcurrencyETagFilter>();
        })
        .AddJsonOptions(options =>
        {
            // Serialize enums as strings so the frontend receives "GeneralLiability" not 6
            options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        });

    // Add Swagger/OpenAPI
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "IBS - Insurance Broker System API",
            Version = "v1",
            Description = "API for managing insurance broker operations including clients, policies, claims, and quotes."
        });

        // Add JWT authentication to Swagger
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });

        // Include XML comments
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
    });

    // Add CORS — explicit allow-lists instead of wildcards
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy
                .WithOrigins(
                    builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                    ?? ["http://localhost:5173", "http://localhost:3000"])
                .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS")
                .WithHeaders("Content-Type", "Authorization", "X-Tenant-Id", "If-Match")
                .AllowCredentials();
        });
    });

    // Add rate limiting (built-in .NET 7+ middleware, no extra package needed)
    builder.Services.AddRateLimiter(rl =>
    {
        // Global: 100 requests per minute per IP
        rl.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
            RateLimitPartition.GetSlidingWindowLimiter(
                partitionKey: ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                    SegmentsPerWindow = 6
                }));

        // Auth-specific: 10 requests per minute per IP for sensitive endpoints
        rl.AddSlidingWindowLimiter("auth", options =>
        {
            options.PermitLimit = 10;
            options.Window = TimeSpan.FromMinutes(1);
            options.SegmentsPerWindow = 3;
        });

        rl.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    });

    // Add health checks
    builder.Services.AddHealthChecks();

    var app = builder.Build();

    // Configure the HTTP request pipeline
    app.UseCorrelationId();
    app.UseGlobalExceptionHandler();

    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "IBS API v1");
            options.RoutePrefix = "swagger";
        });
    }

    // Trust X-Forwarded-For / X-Forwarded-Proto from reverse proxies.
    // KnownNetworks/KnownProxies are cleared so Azure Container Apps ingress
    // (dynamic IP) is trusted to set X-Forwarded-Proto: https, preventing
    // UseHttpsRedirection from redirecting POST requests as a GET (→ 404).
    var forwardedOptions = new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    };
    forwardedOptions.KnownNetworks.Clear();
    forwardedOptions.KnownProxies.Clear();
    app.UseForwardedHeaders(forwardedOptions);

    // Security headers — applied to every response
    app.Use(async (ctx, next) =>
    {
        ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
        ctx.Response.Headers["X-Frame-Options"] = "DENY";
        ctx.Response.Headers["X-XSS-Protection"] = "1; mode=block";
        ctx.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        if (!ctx.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
        {
            ctx.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
            ctx.Response.Headers["Content-Security-Policy"] =
                "default-src 'self'; " +
                "script-src 'self'; " +
                "style-src 'self' 'unsafe-inline'; " +
                "img-src 'self' data:; " +
                "font-src 'self'; " +
                "connect-src 'self'; " +
                "frame-src 'none'; " +
                "object-src 'none'; " +
                "base-uri 'self'; " +
                "form-action 'self'";
        }

        await next();
    });

    app.UseHttpsRedirection();
    app.UseCors();
    app.UseRateLimiter();

    // Tenant resolution before authentication so auth policies can access tenant context
    app.UseTenantResolution();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health");

    // Root endpoint
    app.MapGet("/", () => Results.Ok(new
    {
        Name = "IBS - Insurance Broker System API",
        Version = "1.0.0",
        Status = "Running"
    }));

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>
/// Filters out health check requests from Application Insights telemetry
/// to reduce noise from liveness/readiness probes.
/// </summary>
internal sealed class HealthCheckTelemetryFilter(ITelemetryProcessor next) : ITelemetryProcessor
{
    /// <inheritdoc />
    public void Process(ITelemetry item)
    {
        if (item is RequestTelemetry request &&
            request.Url?.AbsolutePath.Equals("/health", StringComparison.OrdinalIgnoreCase) == true)
        {
            return;
        }

        if (item is TraceTelemetry trace &&
            trace.Message?.Contains("/health", StringComparison.OrdinalIgnoreCase) == true)
        {
            return;
        }

        next.Process(item);
    }
}
