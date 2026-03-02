using FluentAssertions;
using IBS.Claims.Domain.Aggregates.Claim;
using IBS.Claims.Domain.Queries;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Claims.Domain.ValueObjects;
using IBS.Claims.Infrastructure.Persistence;
using IBS.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using ClaimsMoney = IBS.BuildingBlocks.Domain.ValueObjects.Money;

namespace IBS.IntegrationTests.Claims;

/// <summary>
/// Integration tests for ClaimRepository and ClaimQueries using SQL Server via Testcontainers.
/// </summary>
[Collection("Database")]
public class ClaimRepositoryTests : IAsyncLifetime
{
    private readonly SqlServerFixture _fixture;
    private ClaimTestDbContext _context = null!;
    private ClaimRepository _repository = null!;
    private ClaimQueries _queries = null!;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    public ClaimRepositoryTests(SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ClaimTestDbContext>()
            .UseSqlServer(_fixture.GetConnectionString($"ClaimTests_{Guid.NewGuid():N}"))
            .Options;

        _context = new ClaimTestDbContext(options);
        await _context.Database.EnsureCreatedAsync();
        _repository = new ClaimRepository(_context);
        _queries = new ClaimQueries(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task AddAsync_NewClaim_PersistsClaim()
    {
        // Arrange
        var claim = CreateClaim();

        // Act
        await _repository.AddAsync(claim);
        await _context.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(claim.Id);
        retrieved.Should().NotBeNull();
        retrieved!.ClaimNumber.Value.Should().NotBeNullOrEmpty();
        retrieved.Status.Should().Be(ClaimStatus.FNOL);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingClaim_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ClaimWithNotes_ReturnsNotes()
    {
        // Arrange
        var claim = CreateClaim();
        claim.AddNote("First contact with insured", "adjuster@test.com");
        claim.AddNote("Internal review note", "manager@test.com", isInternal: true);

        await _repository.AddAsync(claim);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var retrieved = await _repository.GetByIdAsync(claim.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Notes.Should().HaveCount(2);
        retrieved.Notes.Should().Contain(n => n.IsInternal);
    }

    [Fact]
    public async Task GetByIdAsync_ClaimWithReservesAndPayments_ReturnsAll()
    {
        // Arrange
        var claim = CreateClaim();
        claim.Acknowledge();
        claim.AssignAdjuster("adj-001");
        claim.StartInvestigation();
        claim.Evaluate();
        claim.Approve(ClaimsMoney.USD(25000m));
        claim.SetReserve("Loss", ClaimsMoney.USD(20000m), "adjuster@test.com");
        claim.MoveToSettlement();
        claim.AuthorizePayment("Loss", ClaimsMoney.USD(15000m), "John Doe", DateOnly.FromDateTime(DateTime.UtcNow), "adjuster@test.com");

        await _repository.AddAsync(claim);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var retrieved = await _repository.GetByIdAsync(claim.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Reserves.Should().HaveCount(1);
        retrieved.Payments.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByPolicyIdAsync_MatchingClaims_ReturnsAll()
    {
        // Arrange
        var policyId = Guid.NewGuid();
        var claim1 = CreateClaim(policyId: policyId);
        var claim2 = CreateClaim(policyId: policyId);
        var otherClaim = CreateClaim(policyId: Guid.NewGuid());

        await _repository.AddAsync(claim1);
        await _repository.AddAsync(claim2);
        await _repository.AddAsync(otherClaim);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetByPolicyIdAsync(policyId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(c => c.PolicyId == policyId);
    }

    [Fact]
    public async Task GetByClientIdAsync_MatchingClaims_ReturnsAll()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var claim1 = CreateClaim(clientId: clientId);
        var claim2 = CreateClaim(clientId: clientId);
        var otherClaim = CreateClaim(clientId: Guid.NewGuid());

        await _repository.AddAsync(claim1);
        await _repository.AddAsync(claim2);
        await _repository.AddAsync(otherClaim);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetByClientIdAsync(clientId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(c => c.ClientId == clientId);
    }

    [Fact]
    public async Task GetClaimCountByStatusAsync_MultipleClaims_ReturnsCorrectCounts()
    {
        // Arrange
        var fnolClaim1 = CreateClaim();
        var fnolClaim2 = CreateClaim();
        var acknowledgedClaim = CreateClaim();
        acknowledgedClaim.Acknowledge();

        await _repository.AddAsync(fnolClaim1);
        await _repository.AddAsync(fnolClaim2);
        await _repository.AddAsync(acknowledgedClaim);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetClaimCountByStatusAsync();

        // Assert
        result[ClaimStatus.FNOL].Should().Be(2);
        result[ClaimStatus.Acknowledged].Should().Be(1);
    }

    [Fact]
    public async Task ModifyClaim_AddNoteAndReserve_PersistsChanges()
    {
        // Arrange
        var claim = CreateClaim();
        await _repository.AddAsync(claim);
        await _context.SaveChangesAsync();

        // Act
        var retrieved = await _repository.GetByIdAsync(claim.Id);
        retrieved!.AddNote("Investigation note", "adjuster@test.com");
        retrieved.Acknowledge();
        retrieved.AssignAdjuster("adj-001");
        retrieved.StartInvestigation();
        retrieved.Evaluate();
        retrieved.Approve(ClaimsMoney.USD(10000m));
        retrieved.SetReserve("Loss", ClaimsMoney.USD(10000m), "adjuster@test.com");
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert
        var updated = await _repository.GetByIdAsync(claim.Id);
        updated!.Notes.Should().HaveCount(1);
        updated.Reserves.Should().HaveCount(1);
        updated.Status.Should().Be(ClaimStatus.Approved);
    }

    [Fact]
    public async Task Queries_GetByIdAsync_ReturnsReadModel()
    {
        // Arrange
        var claim = CreateClaim();
        claim.AddNote("Test note", "author@test.com");

        await _repository.AddAsync(claim);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _queries.GetByIdAsync(claim.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(claim.Id);
        result.Status.Should().Be("FNOL");
        result.Notes.Should().HaveCount(1);
    }

    [Fact]
    public async Task Queries_SearchAsync_FiltersByStatus()
    {
        // Arrange
        var fnolClaim = CreateClaim();
        var acknowledgedClaim = CreateClaim();
        acknowledgedClaim.Acknowledge();

        await _repository.AddAsync(fnolClaim);
        await _repository.AddAsync(acknowledgedClaim);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var filter = new ClaimSearchFilter { Status = ClaimStatus.FNOL, PageNumber = 1, PageSize = 20 };
        var result = await _queries.SearchAsync(filter);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Claims.Should().HaveCount(1);
    }

    [Fact]
    public async Task Queries_GetStatisticsAsync_ReturnsAggregates()
    {
        // Arrange
        var claim1 = CreateClaim();
        var claim2 = CreateClaim();
        claim2.Acknowledge();

        await _repository.AddAsync(claim1);
        await _repository.AddAsync(claim2);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _queries.GetStatisticsAsync();

        // Assert
        result.TotalClaims.Should().Be(2);
        result.OpenClaims.Should().Be(2);
        result.ClosedClaims.Should().Be(0);
    }

    private Claim CreateClaim(Guid? policyId = null, Guid? clientId = null)
    {
        return Claim.Create(
            _tenantId,
            policyId ?? Guid.NewGuid(),
            clientId ?? Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddDays(-5),
            DateTimeOffset.UtcNow,
            LossType.PropertyDamage,
            "Water damage to office building from burst pipe",
            _userId,
            estimatedLossAmount: ClaimsMoney.USD(50000m));
    }
}

/// <summary>
/// Test DbContext for Claim integration tests.
/// </summary>
public class ClaimTestDbContext : DbContext
{
    public ClaimTestDbContext(DbContextOptions<ClaimTestDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Gets the claims set.
    /// </summary>
    public DbSet<Claim> Claims => Set<Claim>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply Claim configurations
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(IBS.Claims.Infrastructure.Persistence.Configurations.ClaimConfiguration).Assembly);
    }
}
