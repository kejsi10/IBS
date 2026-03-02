using FluentAssertions;
using IBS.Carriers.Domain.ValueObjects;
using IBS.IntegrationTests.Fixtures;
using IBS.Policies.Domain.Aggregates.Policy;
using IBS.Policies.Domain.Repositories;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Policies.Domain.ValueObjects;
using IBS.Policies.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IBS.IntegrationTests.Policies;

/// <summary>
/// Integration tests for PolicyRepository using SQL Server via Testcontainers.
/// </summary>
[Collection("Database")]
public class PolicyRepositoryTests : IAsyncLifetime
{
    private readonly SqlServerFixture _fixture;
    private PolicyTestDbContext _context = null!;
    private PolicyRepository _repository = null!;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    public PolicyRepositoryTests(SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<PolicyTestDbContext>()
            .UseSqlServer(_fixture.GetConnectionString($"PolicyTests_{Guid.NewGuid():N}"))
            .Options;

        _context = new PolicyTestDbContext(options);
        await _context.Database.EnsureCreatedAsync();
        _repository = new PolicyRepository(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task AddAsync_NewPolicy_PersistsPolicy()
    {
        // Arrange
        var policy = CreatePolicy();

        // Act
        await _repository.AddAsync(policy);
        await _context.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(policy.Id);
        retrieved.Should().NotBeNull();
        retrieved!.PolicyNumber.Value.Should().NotBeNullOrEmpty();
        retrieved.ClientId.Should().Be(policy.ClientId);
        retrieved.CarrierId.Should().Be(policy.CarrierId);
        retrieved.Status.Should().Be(PolicyStatus.Draft);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingPolicy_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_PolicyWithCoverages_ReturnsCoverages()
    {
        // Arrange
        var policy = CreatePolicy();
        policy.AddCoverage("GL", "General Liability", Money.USD(5000m), description: "GL coverage");
        policy.AddCoverage("PD", "Property Damage", Money.USD(3000m));

        await _repository.AddAsync(policy);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var retrieved = await _repository.GetByIdAsync(policy.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Coverages.Should().HaveCount(2);
        retrieved.Coverages.Should().Contain(c => c.Code == "GL");
        retrieved.Coverages.Should().Contain(c => c.Code == "PD");
    }

    [Fact]
    public async Task GetByPolicyNumberAsync_ExistingNumber_ReturnsPolicy()
    {
        // Arrange
        var policy = CreatePolicy(policyNumber: "POL-TEST-001");

        await _repository.AddAsync(policy);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetByPolicyNumberAsync("POL-TEST-001");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(policy.Id);
    }

    [Fact]
    public async Task GetByClientIdAsync_MultiplePolicies_ReturnsAll()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var policy1 = CreatePolicy(clientId: clientId);
        var policy2 = CreatePolicy(clientId: clientId);
        var otherPolicy = CreatePolicy(clientId: Guid.NewGuid());

        await _repository.AddAsync(policy1);
        await _repository.AddAsync(policy2);
        await _repository.AddAsync(otherPolicy);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetByClientIdAsync(clientId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.ClientId == clientId);
    }

    [Fact]
    public async Task GetByStatusAsync_ActivePolicies_ReturnsOnlyActive()
    {
        // Arrange
        var activePolicy = CreatePolicy();
        activePolicy.AddCoverage("GL", "General Liability", Money.USD(5000m));
        activePolicy.Bind(_userId);
        activePolicy.Activate();

        var draftPolicy = CreatePolicy();

        await _repository.AddAsync(activePolicy);
        await _repository.AddAsync(draftPolicy);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetByStatusAsync(PolicyStatus.Active);

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be(activePolicy.Id);
    }

    [Fact]
    public async Task GetExpiringPoliciesAsync_WithinRange_ReturnsMatching()
    {
        // Arrange
        var expiringPolicy = CreatePolicy();
        expiringPolicy.AddCoverage("GL", "General Liability", Money.USD(5000m));
        expiringPolicy.Bind(_userId);
        expiringPolicy.Activate();

        var farFuturePolicy = CreatePolicy(
            effectivePeriod: EffectivePeriod.Create(
                DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
                DateOnly.FromDateTime(DateTime.UtcNow.AddYears(2))));
        farFuturePolicy.AddCoverage("GL", "General Liability", Money.USD(5000m));
        farFuturePolicy.Bind(_userId);
        farFuturePolicy.Activate();

        await _repository.AddAsync(expiringPolicy);
        await _repository.AddAsync(farFuturePolicy);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(400));
        var result = await _repository.GetExpiringPoliciesAsync(startDate, endDate);

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be(expiringPolicy.Id);
    }

    [Fact]
    public async Task PolicyNumberExistsAsync_ExistingNumber_ReturnsTrue()
    {
        // Arrange
        var policy = CreatePolicy(policyNumber: "POL-EXIST-001");
        await _repository.AddAsync(policy);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.PolicyNumberExistsAsync("POL-EXIST-001");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task PolicyNumberExistsAsync_WithExcludeId_ExcludesPolicy()
    {
        // Arrange
        var policy = CreatePolicy(policyNumber: "POL-EXCL-001");
        await _repository.AddAsync(policy);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.PolicyNumberExistsAsync("POL-EXCL-001", policy.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SearchAsync_MatchingTerm_ReturnsResults()
    {
        // Arrange
        var policy1 = CreatePolicy(policyNumber: "POL-SEARCH-001");
        var policy2 = CreatePolicy(policyNumber: "POL-SEARCH-002");
        var policy3 = CreatePolicy(policyNumber: "POL-OTHER-001");

        await _repository.AddAsync(policy1);
        await _repository.AddAsync(policy2);
        await _repository.AddAsync(policy3);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var filter = new PolicySearchFilter { SearchTerm = "SEARCH", PageNumber = 1, PageSize = 10 };
        var result = await _repository.SearchAsync(filter);

        // Assert
        result.TotalCount.Should().Be(2);
        result.Policies.Should().HaveCount(2);
    }

    [Fact]
    public async Task ModifyPolicy_StatusTransition_PersistsChanges()
    {
        // Arrange
        var policy = CreatePolicy();
        policy.AddCoverage("GL", "General Liability", Money.USD(5000m));
        await _repository.AddAsync(policy);
        await _context.SaveChangesAsync();

        // Act
        var retrieved = await _repository.GetByIdAsync(policy.Id);
        retrieved!.Bind(_userId);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert
        var updated = await _repository.GetByIdAsync(policy.Id);
        updated!.Status.Should().Be(PolicyStatus.Bound);
        updated.BoundBy.Should().Be(_userId);
    }

    [Fact]
    public async Task GetByCarrierIdAsync_MultiplePolicies_ReturnsAll()
    {
        // Arrange
        var carrierId = Guid.NewGuid();
        var policy1 = CreatePolicy(carrierId: carrierId);
        var policy2 = CreatePolicy(carrierId: carrierId);
        var otherPolicy = CreatePolicy(carrierId: Guid.NewGuid());

        await _repository.AddAsync(policy1);
        await _repository.AddAsync(policy2);
        await _repository.AddAsync(otherPolicy);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetByCarrierIdAsync(carrierId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.CarrierId == carrierId);
    }

    [Fact]
    public async Task GetByLineOfBusinessAsync_MatchingLob_ReturnsFiltered()
    {
        // Arrange
        var glPolicy = CreatePolicy(lineOfBusiness: LineOfBusiness.GeneralLiability);
        var plPolicy = CreatePolicy(lineOfBusiness: LineOfBusiness.ProfessionalLiability);

        await _repository.AddAsync(glPolicy);
        await _repository.AddAsync(plPolicy);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetByLineOfBusinessAsync(LineOfBusiness.GeneralLiability);

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be(glPolicy.Id);
    }

    [Fact]
    public async Task GetPoliciesDueForRenewalAsync_WithinWindow_ReturnsMatching()
    {
        // Arrange
        var expiringPolicy = CreatePolicy(
            effectivePeriod: EffectivePeriod.Create(
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-330)),
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(35))));
        expiringPolicy.AddCoverage("GL", "General Liability", Money.USD(5000m));
        expiringPolicy.Bind(_userId);
        expiringPolicy.Activate();

        await _repository.AddAsync(expiringPolicy);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetPoliciesDueForRenewalAsync(60);

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be(expiringPolicy.Id);
    }

    [Fact]
    public async Task GetPoliciesDueForRenewalAsync_OutsideWindow_ReturnsEmpty()
    {
        // Arrange
        var farPolicy = CreatePolicy(
            effectivePeriod: EffectivePeriod.Create(
                DateOnly.FromDateTime(DateTime.UtcNow),
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365))));
        farPolicy.AddCoverage("GL", "General Liability", Money.USD(5000m));
        farPolicy.Bind(_userId);
        farPolicy.Activate();

        await _repository.AddAsync(farPolicy);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetPoliciesDueForRenewalAsync(30);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPolicyCountByStatusAsync_MultiplePolicies_ReturnsGroupedCounts()
    {
        // Arrange
        var draftPolicy = CreatePolicy();

        var activePolicy = CreatePolicy();
        activePolicy.AddCoverage("GL", "General Liability", Money.USD(5000m));
        activePolicy.Bind(_userId);
        activePolicy.Activate();

        var boundPolicy = CreatePolicy();
        boundPolicy.AddCoverage("PD", "Property Damage", Money.USD(3000m));
        boundPolicy.Bind(_userId);

        await _repository.AddAsync(draftPolicy);
        await _repository.AddAsync(activePolicy);
        await _repository.AddAsync(boundPolicy);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetPolicyCountByStatusAsync();

        // Assert
        result.Should().ContainKey(PolicyStatus.Draft).WhoseValue.Should().Be(1);
        result.Should().ContainKey(PolicyStatus.Active).WhoseValue.Should().Be(1);
        result.Should().ContainKey(PolicyStatus.Bound).WhoseValue.Should().Be(1);
    }

    [Fact]
    public async Task GetTotalPremiumByLineOfBusinessAsync_FiltersByYear_ReturnsGrouped()
    {
        // Arrange
        var currentYear = DateTime.UtcNow.Year;
        var glPolicy = CreatePolicy(lineOfBusiness: LineOfBusiness.GeneralLiability,
            effectivePeriod: EffectivePeriod.Annual(new DateOnly(currentYear, 1, 1)));
        glPolicy.AddCoverage("GL", "General Liability", Money.USD(5000m));
        glPolicy.Bind(_userId);
        glPolicy.Activate();

        var plPolicy = CreatePolicy(lineOfBusiness: LineOfBusiness.ProfessionalLiability,
            effectivePeriod: EffectivePeriod.Annual(new DateOnly(currentYear, 3, 1)));
        plPolicy.AddCoverage("PL", "Professional Liability", Money.USD(3000m));
        plPolicy.Bind(_userId);
        plPolicy.Activate();

        await _repository.AddAsync(glPolicy);
        await _repository.AddAsync(plPolicy);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetTotalPremiumByLineOfBusinessAsync(currentYear);

        // Assert
        result.Should().ContainKey(LineOfBusiness.GeneralLiability).WhoseValue.Should().Be(5000m);
        result.Should().ContainKey(LineOfBusiness.ProfessionalLiability).WhoseValue.Should().Be(3000m);
    }

    [Fact]
    public async Task AddEndorsement_PersistsEndorsement()
    {
        // Arrange
        var policy = CreatePolicy();
        policy.AddCoverage("GL", "General Liability", Money.USD(5000m));
        policy.Bind(_userId);
        policy.Activate();

        policy.AddEndorsement(
            DateOnly.FromDateTime(DateTime.UtcNow),
            "Coverage Change",
            "Add additional insured",
            Money.USD(500m),
            notes: "Test endorsement");

        await _repository.AddAsync(policy);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var retrieved = await _repository.GetByIdAsync(policy.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Endorsements.Should().HaveCount(1);
        var endorsement = retrieved.Endorsements.First();
        endorsement.Type.Should().Be("Coverage Change");
        endorsement.Description.Should().Be("Add additional insured");
        endorsement.PremiumChange.Amount.Should().Be(500m);
        endorsement.Notes.Should().Be("Test endorsement");
    }

    [Fact]
    public async Task IssueEndorsement_UpdatesTotalPremium()
    {
        // Arrange
        var policy = CreatePolicy();
        policy.AddCoverage("GL", "General Liability", Money.USD(5000m));
        policy.Bind(_userId);
        policy.Activate();

        var endorsement = policy.AddEndorsement(
            DateOnly.FromDateTime(DateTime.UtcNow),
            "Premium Adjustment",
            "Increase coverage limits",
            Money.USD(1000m));
        policy.ApproveEndorsement(endorsement.Id, _userId);
        policy.IssueEndorsement(endorsement.Id);

        await _repository.AddAsync(policy);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var retrieved = await _repository.GetByIdAsync(policy.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.TotalPremium.Amount.Should().Be(6000m);
        var issuedEndorsement = retrieved.Endorsements.First();
        issuedEndorsement.Status.Should().Be(EndorsementStatus.Issued);
    }

    [Fact]
    public async Task RemoveCoverage_DeactivatesCoverage()
    {
        // Arrange
        var policy = CreatePolicy();
        var coverage1 = policy.AddCoverage("GL", "General Liability", Money.USD(5000m));
        policy.AddCoverage("PD", "Property Damage", Money.USD(3000m));

        await _repository.AddAsync(policy);
        await _context.SaveChangesAsync();

        // Act
        var retrieved = await _repository.GetByIdAsync(policy.Id);
        retrieved!.RemoveCoverage(coverage1.Id);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert
        var updated = await _repository.GetByIdAsync(policy.Id);
        updated.Should().NotBeNull();
        updated!.Coverages.Should().HaveCount(2);
        updated.Coverages.Should().Contain(c => c.Code == "GL" && !c.IsActive);
        updated.Coverages.Should().Contain(c => c.Code == "PD" && c.IsActive);
        updated.TotalPremium.Amount.Should().Be(3000m);
    }

    [Fact]
    public async Task CreateRenewal_PersistsBothPolicies()
    {
        // Arrange
        var policy = CreatePolicy();
        policy.AddCoverage("GL", "General Liability", Money.USD(5000m));
        policy.Bind(_userId);
        policy.Activate();

        await _repository.AddAsync(policy);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act — save original status change first, then add renewal in separate tracker scope
        var retrieved = await _repository.GetByIdAsync(policy.Id);
        var renewal = retrieved!.CreateRenewal(_userId);
        retrieved.MarkAsRenewed(renewal.Id, renewal.PolicyNumber.Value);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        await _repository.AddAsync(renewal);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert
        var originalRetrieved = await _repository.GetByIdAsync(policy.Id);
        var renewalRetrieved = await _repository.GetByIdAsync(renewal.Id);

        originalRetrieved.Should().NotBeNull();
        originalRetrieved!.Status.Should().Be(PolicyStatus.Renewed);
        originalRetrieved.RenewalPolicyId.Should().Be(renewal.Id);

        renewalRetrieved.Should().NotBeNull();
        renewalRetrieved!.Status.Should().Be(PolicyStatus.Draft);
        renewalRetrieved.PreviousPolicyId.Should().Be(policy.Id);
        renewalRetrieved.Coverages.Should().HaveCount(1);
        renewalRetrieved.Coverages.First().Code.Should().Be("GL");
    }

    private Policy CreatePolicy(
        Guid? clientId = null,
        Guid? carrierId = null,
        string? policyNumber = null,
        EffectivePeriod? effectivePeriod = null,
        LineOfBusiness lineOfBusiness = LineOfBusiness.GeneralLiability)
    {
        return Policy.Create(
            _tenantId,
            clientId ?? Guid.NewGuid(),
            carrierId ?? Guid.NewGuid(),
            lineOfBusiness,
            lineOfBusiness == LineOfBusiness.GeneralLiability ? "Commercial GL" : "Commercial PL",
            effectivePeriod ?? EffectivePeriod.Annual(DateOnly.FromDateTime(DateTime.UtcNow)),
            _userId,
            policyNumber: policyNumber);
    }
}

/// <summary>
/// Test DbContext for Policy integration tests.
/// </summary>
public class PolicyTestDbContext : DbContext
{
    public PolicyTestDbContext(DbContextOptions<PolicyTestDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Gets the policies set.
    /// </summary>
    public DbSet<Policy> Policies => Set<Policy>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply Policy configurations
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(IBS.Policies.Infrastructure.Persistence.Configurations.PolicyConfiguration).Assembly);
    }
}
