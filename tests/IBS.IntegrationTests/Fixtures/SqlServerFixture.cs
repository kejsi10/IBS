using Testcontainers.MsSql;

namespace IBS.IntegrationTests.Fixtures;

/// <summary>
/// Shared fixture that starts a single SQL Server container for all integration tests.
/// </summary>
public class SqlServerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    /// <summary>
    /// Gets a connection string pointing to a specific database on the shared SQL Server container.
    /// </summary>
    /// <param name="dbName">The database name to use.</param>
    /// <returns>A connection string for the specified database.</returns>
    public string GetConnectionString(string dbName)
    {
        var baseCs = _container.GetConnectionString();
        return baseCs.Replace("Database=master", $"Database={dbName}");
    }

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    /// <inheritdoc />
    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
