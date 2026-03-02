using FluentAssertions;
using IBS.Commissions.Domain.Aggregates.CommissionSchedule;
using IBS.Commissions.Domain.Queries;
using IBS.Commissions.Infrastructure.Persistence;
using IBS.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace IBS.IntegrationTests.Commissions;

/// <summary>
/// Integration tests for CommissionScheduleRepository and CommissionScheduleQueries using SQL Server via Testcontainers.
/// </summary>
[Collection("Database")]
public class CommissionScheduleRepositoryTests : IAsyncLifetime
{
    private readonly SqlServerFixture _fixture;
    private ScheduleTestDbContext _context = null!;
    private CommissionScheduleRepository _repository = null!;
    private CommissionScheduleQueries _queries = null!;
    private readonly Guid _tenantId = Guid.NewGuid();

    public CommissionScheduleRepositoryTests(SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ScheduleTestDbContext>()
            .UseSqlServer(_fixture.GetConnectionString($"ScheduleTests_{Guid.NewGuid():N}"))
            .Options;

        _context = new ScheduleTestDbContext(options);
        await _context.Database.EnsureCreatedAsync();
        _repository = new CommissionScheduleRepository(_context);
        _queries = new CommissionScheduleQueries(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task AddAsync_NewSchedule_PersistsSchedule()
    {
        // Arrange
        var schedule = CreateSchedule();

        // Act
        await _repository.AddAsync(schedule);
        await _context.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(schedule.Id);
        retrieved.Should().NotBeNull();
        retrieved!.CarrierName.Should().Be("Test Carrier");
        retrieved.NewBusinessRate.Should().Be(15m);
        retrieved.RenewalRate.Should().Be(12m);
        retrieved.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingSchedule_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ModifyRates_PersistsChanges()
    {
        // Arrange
        var schedule = CreateSchedule();
        await _repository.AddAsync(schedule);
        await _context.SaveChangesAsync();

        // Act
        var retrieved = await _repository.GetByIdAsync(schedule.Id);
        retrieved!.Update("Test Carrier", "General Liability", 18m, 14m, DateOnly.FromDateTime(DateTime.UtcNow));
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert
        var updated = await _repository.GetByIdAsync(schedule.Id);
        updated!.NewBusinessRate.Should().Be(18m);
        updated.RenewalRate.Should().Be(14m);
    }

    [Fact]
    public async Task UpdateAsync_Deactivate_PersistsIsActive()
    {
        // Arrange
        var schedule = CreateSchedule();
        await _repository.AddAsync(schedule);
        await _context.SaveChangesAsync();

        // Act
        var retrieved = await _repository.GetByIdAsync(schedule.Id);
        retrieved!.Deactivate();
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert
        var updated = await _repository.GetByIdAsync(schedule.Id);
        updated!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Queries_GetByIdAsync_ReturnsReadModel()
    {
        // Arrange
        var schedule = CreateSchedule();
        await _repository.AddAsync(schedule);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _queries.GetByIdAsync(schedule.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(schedule.Id);
        result.CarrierName.Should().Be("Test Carrier");
        result.NewBusinessRate.Should().Be(15m);
    }

    [Fact]
    public async Task Queries_SearchAsync_FiltersByCarrier()
    {
        // Arrange
        var carrierId = Guid.NewGuid();
        var schedule1 = CreateSchedule(carrierId: carrierId);
        var schedule2 = CreateSchedule(carrierId: Guid.NewGuid());

        await _repository.AddAsync(schedule1);
        await _repository.AddAsync(schedule2);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var filter = new ScheduleSearchFilter { CarrierId = carrierId, PageNumber = 1, PageSize = 20 };
        var result = await _queries.SearchAsync(filter);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Schedules.Should().HaveCount(1);
    }

    [Fact]
    public async Task Queries_SearchAsync_FiltersByIsActive()
    {
        // Arrange
        var activeSchedule = CreateSchedule();
        var inactiveSchedule = CreateSchedule();
        inactiveSchedule.Deactivate();

        await _repository.AddAsync(activeSchedule);
        await _repository.AddAsync(inactiveSchedule);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var filter = new ScheduleSearchFilter { IsActive = true, PageNumber = 1, PageSize = 20 };
        var result = await _queries.SearchAsync(filter);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Schedules.Should().OnlyContain(s => s.IsActive);
    }

    [Fact]
    public async Task Queries_SearchAsync_ReturnsPagedResults()
    {
        // Arrange
        for (var i = 0; i < 5; i++)
        {
            await _repository.AddAsync(CreateSchedule(carrierId: Guid.NewGuid()));
        }
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var filter = new ScheduleSearchFilter { PageNumber = 1, PageSize = 3 };
        var result = await _queries.SearchAsync(filter);

        // Assert
        result.TotalCount.Should().Be(5);
        result.Schedules.Should().HaveCount(3);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(3);
    }

    private CommissionSchedule CreateSchedule(Guid? carrierId = null)
    {
        return CommissionSchedule.Create(
            _tenantId,
            carrierId ?? Guid.NewGuid(),
            "Test Carrier",
            "General Liability",
            15m,
            12m,
            DateOnly.FromDateTime(DateTime.UtcNow));
    }
}

/// <summary>
/// Test DbContext for CommissionSchedule integration tests.
/// </summary>
public class ScheduleTestDbContext : DbContext
{
    public ScheduleTestDbContext(DbContextOptions<ScheduleTestDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Gets the commission schedules set.
    /// </summary>
    public DbSet<CommissionSchedule> CommissionSchedules => Set<CommissionSchedule>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply CommissionSchedule configurations
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(IBS.Commissions.Infrastructure.Persistence.Configurations.CommissionScheduleConfiguration).Assembly);
    }
}
