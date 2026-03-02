using FluentAssertions;
using IBS.IntegrationTests.Fixtures;
using IBS.Tenants.Domain.Aggregates.Tenant;
using IBS.Tenants.Domain.ValueObjects;
using IBS.Tenants.Infrastructure.Persistence;
using IBS.Tenants.Infrastructure.Queries;
using Microsoft.EntityFrameworkCore;

namespace IBS.IntegrationTests.Tenants;

/// <summary>
/// Integration tests for TenantRepository using SQL Server via Testcontainers.
/// </summary>
[Collection("Database")]
public class TenantRepositoryTests : IAsyncLifetime
{
    private readonly SqlServerFixture _fixture;
    private TenantTestDbContext _context = null!;
    private TenantRepository _repository = null!;
    private TenantQueries _queries = null!;

    public TenantRepositoryTests(SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<TenantTestDbContext>()
            .UseSqlServer(_fixture.GetConnectionString($"TenantTests_{Guid.NewGuid():N}"))
            .Options;

        _context = new TenantTestDbContext(options);
        await _context.Database.EnsureCreatedAsync();
        _repository = new TenantRepository(_context);
        _queries = new TenantQueries(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task AddAsync_NewTenant_PersistsTenant()
    {
        // Arrange
        var tenant = Tenant.Create("Test Agency", Subdomain.Create("testagency"), SubscriptionTier.Professional);

        // Act
        await _repository.AddAsync(tenant);
        await _context.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(tenant.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Test Agency");
        retrieved.Subdomain.Value.Should().Be("testagency");
        retrieved.SubscriptionTier.Should().Be(SubscriptionTier.Professional);
        retrieved.Status.Should().Be(TenantStatus.Active);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingTenant_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdWithCarriersAsync_TenantWithCarriers_ReturnsCarriers()
    {
        // Arrange
        var tenant = Tenant.Create("Test Agency", Subdomain.Create("testagency"), SubscriptionTier.Professional);
        var carrierId = Guid.NewGuid();
        tenant.AddCarrier(carrierId, "AGC001", 0.15m);

        await _repository.AddAsync(tenant);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var retrieved = await _repository.GetByIdWithCarriersAsync(tenant.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Carriers.Should().HaveCount(1);
        retrieved.Carriers.First().CarrierId.Should().Be(carrierId);
        retrieved.Carriers.First().AgencyCode.Should().Be("AGC001");
        retrieved.Carriers.First().CommissionRate.Should().Be(0.15m);
    }

    [Fact]
    public async Task GetBySubdomainAsync_ExistingSubdomain_ReturnsTenant()
    {
        // Arrange
        var tenant = Tenant.Create("Test Agency", Subdomain.Create("myagency"), SubscriptionTier.Basic);
        await _repository.AddAsync(tenant);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var retrieved = await _repository.GetBySubdomainAsync(Subdomain.Create("myagency"));

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(tenant.Id);
    }

    [Fact]
    public async Task SearchAsync_MatchingTerm_ReturnsResults()
    {
        // Arrange
        var tenant1 = Tenant.Create("Alpha Agency", Subdomain.Create("alpha"), SubscriptionTier.Basic);
        var tenant2 = Tenant.Create("Beta Brokers", Subdomain.Create("beta"), SubscriptionTier.Professional);
        var tenant3 = Tenant.Create("Gamma Agency", Subdomain.Create("gamma"), SubscriptionTier.Enterprise);

        await _repository.AddAsync(tenant1);
        await _repository.AddAsync(tenant2);
        await _repository.AddAsync(tenant3);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _queries.SearchAsync("Agency", 1, 10);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task SearchAsync_NullTerm_ReturnsAll()
    {
        // Arrange
        var tenant1 = Tenant.Create("Alpha Agency", Subdomain.Create("alpha"), SubscriptionTier.Basic);
        var tenant2 = Tenant.Create("Beta Brokers", Subdomain.Create("beta"), SubscriptionTier.Professional);

        await _repository.AddAsync(tenant1);
        await _repository.AddAsync(tenant2);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _queries.SearchAsync(null, 1, 10);

        // Assert
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_QueryDto_ReturnsTenantDetails()
    {
        // Arrange
        var tenant = Tenant.Create("Details Agency", Subdomain.Create("details"), SubscriptionTier.Enterprise);
        await _repository.AddAsync(tenant);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _queries.GetByIdAsync(tenant.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Details Agency");
        result.Subdomain.Should().Be("details");
        result.SubscriptionTier.Should().Be("Enterprise");
        result.Status.Should().Be("Active");
    }

    [Fact]
    public async Task SubdomainExistsAsync_ExistingSubdomain_ReturnsTrue()
    {
        // Arrange
        var tenant = Tenant.Create("Test Agency", Subdomain.Create("existing"), SubscriptionTier.Basic);
        await _repository.AddAsync(tenant);
        await _context.SaveChangesAsync();

        // Act
        var result = await _queries.SubdomainExistsAsync("existing");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SubdomainExistsAsync_WithExcludeId_ExcludesTenant()
    {
        // Arrange
        var tenant = Tenant.Create("Test Agency", Subdomain.Create("existing"), SubscriptionTier.Basic);
        await _repository.AddAsync(tenant);
        await _context.SaveChangesAsync();

        // Act
        var result = await _queries.SubdomainExistsAsync("existing", tenant.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ModifyTenant_StatusTransitions_PersistsChanges()
    {
        // Arrange
        var tenant = Tenant.Create("Test Agency", Subdomain.Create("statustst"), SubscriptionTier.Professional);
        await _repository.AddAsync(tenant);
        await _context.SaveChangesAsync();

        // Act - Suspend
        var retrieved = await _repository.GetByIdAsync(tenant.Id);
        retrieved!.Suspend();
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert - Suspended
        var suspended = await _queries.GetByIdAsync(tenant.Id);
        suspended!.Status.Should().Be("Suspended");

        // Act - Activate
        var toActivate = await _repository.GetByIdAsync(tenant.Id);
        toActivate!.Activate();
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert - Active again
        var activated = await _queries.GetByIdAsync(tenant.Id);
        activated!.Status.Should().Be("Active");
    }
}

/// <summary>
/// Test DbContext for Tenant integration tests.
/// </summary>
public class TenantTestDbContext : DbContext
{
    public TenantTestDbContext(DbContextOptions<TenantTestDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Gets the tenants set.
    /// </summary>
    public DbSet<Tenant> Tenants => Set<Tenant>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply Tenant configurations
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(IBS.Tenants.Infrastructure.Persistence.Configurations.TenantConfiguration).Assembly);
    }
}
