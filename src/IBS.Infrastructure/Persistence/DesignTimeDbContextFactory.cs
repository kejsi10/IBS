using IBS.BuildingBlocks.Infrastructure.Multitenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace IBS.Infrastructure.Persistence;

/// <summary>
/// Factory for creating IbsDbContext at design time for EF Core tools (migrations, etc.)
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<IbsDbContext>
{
    /// <summary>
    /// Creates a new instance of IbsDbContext for design-time operations.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>A configured IbsDbContext instance.</returns>
    public IbsDbContext CreateDbContext(string[] args)
    {
        // Try to get connection string from command line args first
        var connectionString = GetConnectionStringFromArgs(args);

        // If not provided, try to load from configuration
        if (string.IsNullOrEmpty(connectionString))
        {
            var basePath = Directory.GetCurrentDirectory();

            // Check if we're in the IBS.Api directory or need to navigate there
            var apiPath = basePath.EndsWith("IBS.Api")
                ? basePath
                : Path.Combine(basePath, "src", "IBS.Api");

            if (Directory.Exists(apiPath))
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(apiPath)
                    .AddJsonFile("appsettings.json", optional: true)
                    .AddJsonFile("appsettings.Development.json", optional: true)
                    .Build();

                connectionString = configuration.GetConnectionString("DefaultConnection");
            }
        }

        // Fallback to default if still not found
        if (string.IsNullOrEmpty(connectionString))
        {
            connectionString = "Server=localhost,1433;Database=IBS;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True";
        }

        var optionsBuilder = new DbContextOptionsBuilder<IbsDbContext>();
        optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.MigrationsAssembly(typeof(IbsDbContext).Assembly.FullName);
        });

        // Create a design-time tenant context (no tenant filtering at design time)
        var tenantContext = new DesignTimeTenantContext();

        return new IbsDbContext(optionsBuilder.Options, tenantContext);
    }

    private static string? GetConnectionStringFromArgs(string[] args)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i].Equals("--connection", StringComparison.OrdinalIgnoreCase))
            {
                return args[i + 1];
            }
        }
        return null;
    }

    /// <summary>
    /// A simple tenant context for design-time operations (no tenant filtering).
    /// </summary>
    private sealed class DesignTimeTenantContext : ITenantContext
    {
        /// <inheritdoc />
        public Guid TenantId => Guid.Empty;

        /// <inheritdoc />
        public bool HasTenant => false;
    }
}
