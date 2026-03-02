namespace IBS.IntegrationTests.Fixtures;

/// <summary>
/// Collection definition that shares a single SQL Server container across all test classes.
/// </summary>
[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<SqlServerFixture>;
