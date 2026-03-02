using FluentAssertions;
using IBS.Commissions.Domain.Aggregates.CommissionStatement;
using IBS.Commissions.Domain.Queries;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Commissions.Domain.ValueObjects;
using IBS.Commissions.Infrastructure.Persistence;
using IBS.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using CommissionMoney = IBS.BuildingBlocks.Domain.ValueObjects.Money;

namespace IBS.IntegrationTests.Commissions;

/// <summary>
/// Integration tests for CommissionStatementRepository and CommissionStatementQueries using SQL Server via Testcontainers.
/// </summary>
[Collection("Database")]
public class CommissionStatementRepositoryTests : IAsyncLifetime
{
    private readonly SqlServerFixture _fixture;
    private StatementTestDbContext _context = null!;
    private CommissionStatementRepository _repository = null!;
    private CommissionStatementQueries _queries = null!;
    private readonly Guid _tenantId = Guid.NewGuid();

    public CommissionStatementRepositoryTests(SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<StatementTestDbContext>()
            .UseSqlServer(_fixture.GetConnectionString($"StatementTests_{Guid.NewGuid():N}"))
            .Options;

        _context = new StatementTestDbContext(options);
        await _context.Database.EnsureCreatedAsync();
        _repository = new CommissionStatementRepository(_context);
        _queries = new CommissionStatementQueries(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task AddAsync_NewStatement_PersistsStatement()
    {
        // Arrange
        var statement = CreateStatement();

        // Act
        await _repository.AddAsync(statement);
        await _context.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(statement.Id);
        retrieved.Should().NotBeNull();
        retrieved!.StatementNumber.Should().Be("STMT-001");
        retrieved.Status.Should().Be(StatementStatus.Received);
        retrieved.TotalCommission.Amount.Should().Be(5000m);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingStatement_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_StatementWithLineItems_ReturnsLineItems()
    {
        // Arrange
        var statement = CreateStatement();
        statement.AddLineItem(
            "POL-001", "Acme Corp", "General Liability",
            DateOnly.FromDateTime(DateTime.UtcNow),
            TransactionType.NewBusiness,
            CommissionMoney.USD(10000m), 15m, CommissionMoney.USD(1500m));
        statement.AddLineItem(
            "POL-002", "Beta Inc", "Property",
            DateOnly.FromDateTime(DateTime.UtcNow),
            TransactionType.Renewal,
            CommissionMoney.USD(8000m), 12m, CommissionMoney.USD(960m));

        await _repository.AddAsync(statement);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var retrieved = await _repository.GetByIdAsync(statement.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.LineItems.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_StatementWithProducerSplits_ReturnsSplits()
    {
        // Arrange
        var statement = CreateStatement();
        var lineItem = statement.AddLineItem(
            "POL-001", "Acme Corp", "General Liability",
            DateOnly.FromDateTime(DateTime.UtcNow),
            TransactionType.NewBusiness,
            CommissionMoney.USD(10000m), 15m, CommissionMoney.USD(1500m));
        statement.AddProducerSplit(
            lineItem.Id, "Agent Smith", Guid.NewGuid(), 60m, CommissionMoney.USD(900m));

        await _repository.AddAsync(statement);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var retrieved = await _repository.GetByIdAsync(statement.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.ProducerSplits.Should().HaveCount(1);
        retrieved.ProducerSplits.First().ProducerName.Should().Be("Agent Smith");
    }

    [Fact]
    public async Task UpdateAsync_AddLineItemAndSplit_PersistsChanges()
    {
        // Arrange
        var statement = CreateStatement();
        await _repository.AddAsync(statement);
        await _context.SaveChangesAsync();

        // Act
        var retrieved = await _repository.GetByIdAsync(statement.Id);
        var lineItem = retrieved!.AddLineItem(
            "POL-003", "Gamma LLC", "Auto",
            DateOnly.FromDateTime(DateTime.UtcNow),
            TransactionType.NewBusiness,
            CommissionMoney.USD(5000m), 10m, CommissionMoney.USD(500m));
        retrieved.AddProducerSplit(
            lineItem.Id, "Agent Jones", Guid.NewGuid(), 50m, CommissionMoney.USD(250m));
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert
        var updated = await _repository.GetByIdAsync(statement.Id);
        updated!.LineItems.Should().HaveCount(1);
        updated.ProducerSplits.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateAsync_StatusTransition_PersistsStatus()
    {
        // Arrange
        var statement = CreateStatement();
        await _repository.AddAsync(statement);
        await _context.SaveChangesAsync();

        // Act
        var retrieved = await _repository.GetByIdAsync(statement.Id);
        retrieved!.StartReconciling();
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert
        var updated = await _repository.GetByIdAsync(statement.Id);
        updated!.Status.Should().Be(StatementStatus.Reconciling);
    }

    [Fact]
    public async Task Queries_GetByIdAsync_ReturnsReadModel()
    {
        // Arrange
        var statement = CreateStatement();
        statement.AddLineItem(
            "POL-001", "Acme Corp", "General Liability",
            DateOnly.FromDateTime(DateTime.UtcNow),
            TransactionType.NewBusiness,
            CommissionMoney.USD(10000m), 15m, CommissionMoney.USD(1500m));

        await _repository.AddAsync(statement);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _queries.GetByIdAsync(statement.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(statement.Id);
        result.StatementNumber.Should().Be("STMT-001");
        result.LineItems.Should().HaveCount(1);
    }

    [Fact]
    public async Task Queries_SearchAsync_FiltersByStatus()
    {
        // Arrange
        var receivedStatement = CreateStatement(statementNumber: "STMT-R01");
        var reconcilingStatement = CreateStatement(statementNumber: "STMT-R02");
        reconcilingStatement.StartReconciling();

        await _repository.AddAsync(receivedStatement);
        await _repository.AddAsync(reconcilingStatement);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var filter = new StatementSearchFilter { Status = StatementStatus.Received, PageNumber = 1, PageSize = 20 };
        var result = await _queries.SearchAsync(filter);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Statements.Should().HaveCount(1);
    }

    [Fact]
    public async Task Queries_SearchAsync_FiltersByPeriod()
    {
        // Arrange
        var janStatement = CreateStatement(statementNumber: "STMT-JAN", periodMonth: 1, periodYear: 2025);
        var febStatement = CreateStatement(statementNumber: "STMT-FEB", periodMonth: 2, periodYear: 2025);

        await _repository.AddAsync(janStatement);
        await _repository.AddAsync(febStatement);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var filter = new StatementSearchFilter { PeriodMonth = 1, PeriodYear = 2025, PageNumber = 1, PageSize = 20 };
        var result = await _queries.SearchAsync(filter);

        // Assert
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Queries_GetStatisticsAsync_ReturnsAggregates()
    {
        // Arrange
        var statement1 = CreateStatement(statementNumber: "STMT-S01");
        var statement2 = CreateStatement(statementNumber: "STMT-S02");

        // Add line items and reconcile them before marking statement as reconciled
        var lineItem = statement2.AddLineItem(
            "POL-001", "Acme Corp", "General Liability",
            DateOnly.FromDateTime(DateTime.UtcNow),
            TransactionType.NewBusiness,
            CommissionMoney.USD(10000m), 15m, CommissionMoney.USD(1500m));
        statement2.StartReconciling();
        statement2.ReconcileLineItem(lineItem.Id);
        statement2.MarkReconciled();
        statement2.MarkPaid();

        await _repository.AddAsync(statement1);
        await _repository.AddAsync(statement2);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _queries.GetStatisticsAsync();

        // Assert
        result.TotalStatements.Should().Be(2);
        result.ReceivedStatements.Should().Be(1);
        result.PaidStatements.Should().Be(1);
        result.TotalCommissionAmount.Should().Be(10000m);
    }

    [Fact]
    public async Task Queries_GetSummaryReportAsync_GroupsByCarrierPeriod()
    {
        // Arrange
        var carrierId = Guid.NewGuid();
        var statement1 = CreateStatement(statementNumber: "STMT-G01", carrierId: carrierId, periodMonth: 1, periodYear: 2025);
        var statement2 = CreateStatement(statementNumber: "STMT-G02", carrierId: carrierId, periodMonth: 1, periodYear: 2025);

        await _repository.AddAsync(statement1);
        await _repository.AddAsync(statement2);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _queries.GetSummaryReportAsync(carrierId: carrierId, periodMonth: 1, periodYear: 2025);

        // Assert
        result.Should().HaveCount(1);
        result[0].StatementCount.Should().Be(2);
        result[0].TotalCommission.Should().Be(10000m);
    }

    [Fact]
    public async Task Queries_GetProducerReportAsync_GroupsByProducer()
    {
        // Arrange
        var producerId = Guid.NewGuid();
        var statement = CreateStatement(statementNumber: "STMT-P01");
        var lineItem = statement.AddLineItem(
            "POL-001", "Acme Corp", "General Liability",
            DateOnly.FromDateTime(DateTime.UtcNow),
            TransactionType.NewBusiness,
            CommissionMoney.USD(10000m), 15m, CommissionMoney.USD(1500m));
        statement.AddProducerSplit(lineItem.Id, "Agent Smith", producerId, 60m, CommissionMoney.USD(900m));

        await _repository.AddAsync(statement);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _queries.GetProducerReportAsync(producerId: producerId);

        // Assert
        result.Should().HaveCount(1);
        result[0].ProducerName.Should().Be("Agent Smith");
        result[0].TotalSplitAmount.Should().Be(900m);
    }

    private CommissionStatement CreateStatement(
        string statementNumber = "STMT-001",
        Guid? carrierId = null,
        int periodMonth = 1,
        int periodYear = 2025)
    {
        return CommissionStatement.Create(
            _tenantId,
            carrierId ?? Guid.NewGuid(),
            "Test Carrier",
            statementNumber,
            periodMonth,
            periodYear,
            DateOnly.FromDateTime(DateTime.UtcNow),
            CommissionMoney.USD(50000m),
            CommissionMoney.USD(5000m));
    }
}

/// <summary>
/// Test DbContext for CommissionStatement integration tests.
/// </summary>
public class StatementTestDbContext : DbContext
{
    public StatementTestDbContext(DbContextOptions<StatementTestDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Gets the commission statements set.
    /// </summary>
    public DbSet<CommissionStatement> CommissionStatements => Set<CommissionStatement>();

    /// <summary>
    /// Gets the commission line items set.
    /// </summary>
    public DbSet<CommissionLineItem> CommissionLineItems => Set<CommissionLineItem>();

    /// <summary>
    /// Gets the producer splits set.
    /// </summary>
    public DbSet<ProducerSplit> ProducerSplits => Set<ProducerSplit>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply CommissionStatement configurations
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(IBS.Commissions.Infrastructure.Persistence.Configurations.CommissionStatementConfiguration).Assembly);
    }
}
