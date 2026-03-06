using FluentAssertions;
using IBS.BuildingBlocks.Domain;
using IBS.Carriers.Domain.ValueObjects;
using IBS.Policies.Domain.Aggregates.Policy;
using IBS.Policies.Domain.Events;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Policies.Domain.ValueObjects;

namespace IBS.UnitTests.Policies.Domain;

/// <summary>
/// Unit tests for the Policy aggregate root.
/// </summary>
public class PolicyTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _clientId = Guid.NewGuid();
    private readonly Guid _carrierId = Guid.NewGuid();

    [Fact]
    public void Create_ValidInputs_CreatesPolicy()
    {
        // Arrange
        var effectivePeriod = EffectivePeriod.Annual(new DateOnly(2024, 1, 1));

        // Act
        var policy = Policy.Create(
            _tenantId,
            _clientId,
            _carrierId,
            LineOfBusiness.PersonalAuto,
            "Personal Auto Policy",
            effectivePeriod,
            _userId);

        // Assert
        policy.Should().NotBeNull();
        policy.TenantId.Should().Be(_tenantId);
        policy.ClientId.Should().Be(_clientId);
        policy.CarrierId.Should().Be(_carrierId);
        policy.LineOfBusiness.Should().Be(LineOfBusiness.PersonalAuto);
        policy.PolicyType.Should().Be("Personal Auto Policy");
        policy.Status.Should().Be(PolicyStatus.Draft);
        policy.EffectivePeriod.Should().Be(effectivePeriod);
        policy.TotalPremium.Amount.Should().Be(0m);
        policy.CreatedBy.Should().Be(_userId);
        policy.DomainEvents.Should().HaveCount(1);
        policy.DomainEvents.First().Should().BeOfType<PolicyCreatedEvent>();
    }

    [Fact]
    public void Create_WithCustomPolicyNumber_UsesPolicyNumber()
    {
        // Arrange
        var effectivePeriod = EffectivePeriod.Annual(new DateOnly(2024, 1, 1));
        var policyNumber = "CUSTOM-2024-001";

        // Act
        var policy = Policy.Create(
            _tenantId,
            _clientId,
            _carrierId,
            LineOfBusiness.PersonalAuto,
            "Personal Auto Policy",
            effectivePeriod,
            _userId,
            policyNumber: policyNumber);

        // Assert
        policy.PolicyNumber.Value.Should().Be(policyNumber);
    }

    [Fact]
    public void Create_WithoutPolicyNumber_GeneratesPolicyNumber()
    {
        // Arrange
        var effectivePeriod = EffectivePeriod.Annual(new DateOnly(2024, 1, 1));

        // Act
        var policy = Policy.Create(
            _tenantId,
            _clientId,
            _carrierId,
            LineOfBusiness.PersonalAuto,
            "Personal Auto Policy",
            effectivePeriod,
            _userId);

        // Assert
        policy.PolicyNumber.Value.Should().StartWith("PER-");
    }

    [Fact]
    public void Create_EmptyPolicyType_ThrowsArgumentException()
    {
        // Arrange
        var effectivePeriod = EffectivePeriod.Annual(new DateOnly(2024, 1, 1));

        // Act
        var act = () => Policy.Create(
            _tenantId,
            _clientId,
            _carrierId,
            LineOfBusiness.PersonalAuto,
            "",
            effectivePeriod,
            _userId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Policy type*required*");
    }

    [Fact]
    public void AddCoverage_ValidInputs_AddsCoverage()
    {
        // Arrange
        var policy = CreateTestPolicy();
        policy.ClearDomainEvents();
        var premium = Money.Create(500m);

        // Act
        var coverage = policy.AddCoverage("BI", "Bodily Injury", premium);

        // Assert
        coverage.Should().NotBeNull();
        coverage.Code.Should().Be("BI");
        coverage.Name.Should().Be("Bodily Injury");
        coverage.PremiumAmount.Should().Be(premium);
        policy.Coverages.Should().HaveCount(1);
        policy.TotalPremium.Amount.Should().Be(500m);
        policy.DomainEvents.Should().HaveCount(2); // CoverageAdded + PremiumChanged
        policy.DomainEvents.Should().ContainItemsAssignableTo<CoverageAddedEvent>();
    }

    [Fact]
    public void AddCoverage_MultipleCoverages_CalculatesTotalPremium()
    {
        // Arrange
        var policy = CreateTestPolicy();
        var premium1 = Money.Create(500m);
        var premium2 = Money.Create(300m);

        // Act
        policy.AddCoverage("BI", "Bodily Injury", premium1);
        policy.AddCoverage("PD", "Property Damage", premium2);

        // Assert
        policy.Coverages.Should().HaveCount(2);
        policy.TotalPremium.Amount.Should().Be(800m);
    }

    [Fact]
    public void AddCoverage_DuplicateCode_ThrowsBusinessRuleViolationException()
    {
        // Arrange
        var policy = CreateTestPolicy();
        policy.AddCoverage("BI", "Bodily Injury", Money.Create(500m));

        // Act
        var act = () => policy.AddCoverage("BI", "Another Bodily Injury", Money.Create(600m));

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public void AddCoverage_PolicyNotDraft_ThrowsBusinessRuleViolationException()
    {
        // Arrange
        var policy = CreateTestPolicy();
        policy.AddCoverage("BI", "Bodily Injury", Money.Create(500m));
        policy.Bind(_userId);
        policy.Activate();

        // Act
        var act = () => policy.AddCoverage("PD", "Property Damage", Money.Create(300m));

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*Cannot add coverage*");
    }

    [Fact]
    public void RemoveCoverage_ExistingCoverage_DeactivatesCoverage()
    {
        // Arrange
        var policy = CreateTestPolicy();
        var coverage = policy.AddCoverage("BI", "Bodily Injury", Money.Create(500m));
        policy.ClearDomainEvents();

        // Act
        policy.RemoveCoverage(coverage.Id);

        // Assert
        coverage.IsActive.Should().BeFalse();
        policy.TotalPremium.Amount.Should().Be(0m);
        policy.DomainEvents.Should().ContainItemsAssignableTo<CoverageRemovedEvent>();
    }

    [Fact]
    public void RemoveCoverage_NonExistingCoverage_ThrowsBusinessRuleViolationException()
    {
        // Arrange
        var policy = CreateTestPolicy();

        // Act
        var act = () => policy.RemoveCoverage(Guid.NewGuid());

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public void Bind_DraftPolicyWithCoverage_BindsPolicy()
    {
        // Arrange
        var policy = CreateTestPolicy();
        policy.AddCoverage("BI", "Bodily Injury", Money.Create(500m));
        policy.ClearDomainEvents();

        // Act
        policy.Bind(_userId);

        // Assert
        policy.Status.Should().Be(PolicyStatus.Bound);
        policy.BoundAt.Should().NotBeNull();
        policy.BoundBy.Should().Be(_userId);
        policy.DomainEvents.Should().HaveCount(1);
        policy.DomainEvents.First().Should().BeOfType<PolicyBoundEvent>();
    }

    [Fact]
    public void Bind_DraftPolicyWithoutCoverage_ThrowsBusinessRuleViolationException()
    {
        // Arrange
        var policy = CreateTestPolicy();

        // Act
        var act = () => policy.Bind(_userId);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*must have at least one coverage*");
    }

    [Fact]
    public void Bind_NonDraftPolicy_ThrowsBusinessRuleViolationException()
    {
        // Arrange
        var policy = CreateTestPolicy();
        policy.AddCoverage("BI", "Bodily Injury", Money.Create(500m));
        policy.Bind(_userId);

        // Act
        var act = () => policy.Bind(_userId);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*Only draft policies*");
    }

    [Fact]
    public void Activate_BoundPolicy_ActivatesPolicy()
    {
        // Arrange
        var policy = CreateTestPolicy();
        policy.AddCoverage("BI", "Bodily Injury", Money.Create(500m));
        policy.Bind(_userId);
        policy.ClearDomainEvents();

        // Act
        policy.Activate();

        // Assert
        policy.Status.Should().Be(PolicyStatus.Active);
        policy.DomainEvents.Should().HaveCount(1);
        policy.DomainEvents.First().Should().BeOfType<PolicyActivatedEvent>();
    }

    [Fact]
    public void Activate_NonBoundPolicy_ThrowsBusinessRuleViolationException()
    {
        // Arrange
        var policy = CreateTestPolicy();

        // Act
        var act = () => policy.Activate();

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*Only bound policies*");
    }

    [Fact]
    public void Cancel_ActivePolicy_CancelsPolicy()
    {
        // Arrange
        var policy = CreateTestPolicy();
        policy.AddCoverage("BI", "Bodily Injury", Money.Create(500m));
        policy.Bind(_userId);
        policy.Activate();
        policy.ClearDomainEvents();

        var cancellationDate = policy.EffectivePeriod.EffectiveDate.AddMonths(6);

        // Act
        policy.Cancel(cancellationDate, "Insured request", CancellationType.InsuredRequest);

        // Assert
        policy.Status.Should().Be(PolicyStatus.Cancelled);
        policy.CancellationDate.Should().Be(cancellationDate);
        policy.CancellationReason.Should().Be("Insured request");
        policy.CancellationType.Should().Be(CancellationType.InsuredRequest);
        policy.DomainEvents.Should().HaveCount(1);
        policy.DomainEvents.First().Should().BeOfType<PolicyCancelledEvent>();
    }

    [Fact]
    public void Cancel_AlreadyCancelledPolicy_ThrowsBusinessRuleViolationException()
    {
        // Arrange
        var policy = CreateTestPolicy();
        policy.AddCoverage("BI", "Bodily Injury", Money.Create(500m));
        policy.Bind(_userId);
        policy.Activate();

        var cancellationDate = policy.EffectivePeriod.EffectiveDate.AddMonths(6);
        policy.Cancel(cancellationDate, "First cancellation", CancellationType.InsuredRequest);

        // Act
        var act = () => policy.Cancel(cancellationDate, "Second cancellation", CancellationType.InsuredRequest);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*terminal state*");
    }

    [Fact]
    public void Cancel_DateBeforeEffective_ThrowsBusinessRuleViolationException()
    {
        // Arrange
        var policy = CreateTestPolicy();
        policy.AddCoverage("BI", "Bodily Injury", Money.Create(500m));
        policy.Bind(_userId);
        policy.Activate();

        var cancellationDate = policy.EffectivePeriod.EffectiveDate.AddDays(-1);

        // Act
        var act = () => policy.Cancel(cancellationDate, "Invalid cancellation", CancellationType.InsuredRequest);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*cannot be before*effective date*");
    }

    [Fact]
    public void Cancel_OnEffectiveDate_SetsFlatCancel()
    {
        // Arrange
        var policy = CreateTestPolicy();
        policy.AddCoverage("BI", "Bodily Injury", Money.Create(500m));
        policy.Bind(_userId);
        policy.Activate();

        // Act
        policy.Cancel(policy.EffectivePeriod.EffectiveDate, "Never needed", CancellationType.InsuredRequest);

        // Assert
        policy.CancellationType.Should().Be(CancellationType.FlatCancel);
    }

    [Fact]
    public void AddEndorsement_ValidInputs_AddsEndorsement()
    {
        // Arrange
        var policy = CreateTestPolicy();
        policy.AddCoverage("BI", "Bodily Injury", Money.Create(500m));
        policy.ClearDomainEvents();

        var effectiveDate = policy.EffectivePeriod.EffectiveDate.AddMonths(3);
        var premiumChange = Money.CreateWithSign(50m);

        // Act
        var endorsement = policy.AddEndorsement(
            effectiveDate,
            "Additional Driver",
            "Added new driver to policy",
            premiumChange);

        // Assert
        endorsement.Should().NotBeNull();
        endorsement.EndorsementNumber.Should().Contain("-END001");
        endorsement.EffectiveDate.Should().Be(effectiveDate);
        endorsement.Type.Should().Be("Additional Driver");
        endorsement.Description.Should().Be("Added new driver to policy");
        endorsement.PremiumChange.Should().Be(premiumChange);
        endorsement.Status.Should().Be(EndorsementStatus.Pending);
        policy.Endorsements.Should().HaveCount(1);
        policy.DomainEvents.Should().ContainItemsAssignableTo<EndorsementAddedEvent>();
    }

    [Fact]
    public void AddEndorsement_DateOutsidePolicyPeriod_ThrowsBusinessRuleViolationException()
    {
        // Arrange
        var policy = CreateTestPolicy();
        policy.AddCoverage("BI", "Bodily Injury", Money.Create(500m));

        var effectiveDate = policy.EffectivePeriod.ExpirationDate.AddDays(1);

        // Act
        var act = () => policy.AddEndorsement(
            effectiveDate,
            "Invalid",
            "Outside policy period",
            Money.CreateWithSign(50m));

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*within the policy period*");
    }

    [Fact]
    public void IssueEndorsement_ApprovedEndorsement_AppliesPremiumChange()
    {
        // Arrange
        var policy = CreateTestPolicy();
        policy.AddCoverage("BI", "Bodily Injury", Money.Create(500m));

        var endorsement = policy.AddEndorsement(
            policy.EffectivePeriod.EffectiveDate.AddMonths(3),
            "Additional Driver",
            "Added new driver",
            Money.CreateWithSign(100m));

        policy.ApproveEndorsement(endorsement.Id, _userId);
        policy.ClearDomainEvents();

        // Act
        policy.IssueEndorsement(endorsement.Id);

        // Assert
        endorsement.Status.Should().Be(EndorsementStatus.Issued);
        policy.TotalPremium.Amount.Should().Be(600m); // 500 + 100
        policy.DomainEvents.Should().ContainItemsAssignableTo<PolicyPremiumChangedEvent>();
    }

    [Fact]
    public void CreateRenewal_ActivePolicy_CreatesRenewalPolicy()
    {
        // Arrange
        var policy = CreateTestPolicy();
        policy.AddCoverage("BI", "Bodily Injury", Money.Create(500m));
        policy.AddCoverage("PD", "Property Damage", Money.Create(300m));
        policy.Bind(_userId);
        policy.Activate();

        // Act
        var renewal = policy.CreateRenewal(_userId);

        // Assert
        renewal.Should().NotBeNull();
        renewal.Status.Should().Be(PolicyStatus.Draft);
        renewal.PreviousPolicyId.Should().Be(policy.Id);
        renewal.ClientId.Should().Be(policy.ClientId);
        renewal.CarrierId.Should().Be(policy.CarrierId);
        renewal.LineOfBusiness.Should().Be(policy.LineOfBusiness);
        renewal.EffectivePeriod.EffectiveDate.Should().Be(policy.EffectivePeriod.ExpirationDate);
        renewal.Coverages.Should().HaveCount(2);
        renewal.TotalPremium.Amount.Should().Be(800m);
    }

    [Fact]
    public void CreateRenewal_DraftPolicy_ThrowsBusinessRuleViolationException()
    {
        // Arrange
        var policy = CreateTestPolicy();

        // Act
        var act = () => policy.CreateRenewal(_userId);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*active or pending renewal*");
    }

    [Fact]
    public void SetPendingRenewal_ActivePolicy_SetsStatus()
    {
        // Arrange
        var policy = CreateTestPolicy();
        policy.AddCoverage("BI", "Bodily Injury", Money.Create(500m));
        policy.Bind(_userId);
        policy.Activate();

        // Act
        policy.SetPendingRenewal();

        // Assert
        policy.Status.Should().Be(PolicyStatus.PendingRenewal);
    }

    [Fact]
    public void NonRenew_ActivePolicy_SetsNonRenewedStatus()
    {
        // Arrange
        var policy = CreateTestPolicy();
        policy.AddCoverage("BI", "Bodily Injury", Money.Create(500m));
        policy.Bind(_userId);
        policy.Activate();

        // Act
        policy.NonRenew("Multiple claims in term");

        // Assert
        policy.Status.Should().Be(PolicyStatus.NonRenewed);
        policy.Notes.Should().Contain("Multiple claims in term");
    }

    [Fact]
    public void IsInForce_ActivePolicyInPeriod_ReturnsTrue()
    {
        // Arrange
        var effectivePeriod = EffectivePeriod.Create(
            DateOnly.FromDateTime(DateTime.Today.AddMonths(-6)),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(6)));

        var policy = Policy.Create(
            _tenantId,
            _clientId,
            _carrierId,
            LineOfBusiness.PersonalAuto,
            "Personal Auto Policy",
            effectivePeriod,
            _userId);

        policy.AddCoverage("BI", "Bodily Injury", Money.Create(500m));
        policy.Bind(_userId);
        policy.Activate();

        // Act
        var result = policy.IsInForce();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GetTotalEndorsementPremium_IssuedEndorsements_ReturnsTotalChange()
    {
        // Arrange
        var policy = CreateTestPolicy();
        policy.AddCoverage("BI", "Bodily Injury", Money.Create(500m));

        var endorsement1 = policy.AddEndorsement(
            policy.EffectivePeriod.EffectiveDate.AddMonths(1),
            "Add Driver",
            "Added driver",
            Money.CreateWithSign(100m));

        var endorsement2 = policy.AddEndorsement(
            policy.EffectivePeriod.EffectiveDate.AddMonths(2),
            "Remove Coverage",
            "Removed optional coverage",
            Money.CreateWithSign(-50m));

        policy.ApproveEndorsement(endorsement1.Id, _userId);
        policy.IssueEndorsement(endorsement1.Id);
        policy.ApproveEndorsement(endorsement2.Id, _userId);
        policy.IssueEndorsement(endorsement2.Id);

        // Act
        var totalEndorsementPremium = policy.GetTotalEndorsementPremium();

        // Assert
        totalEndorsementPremium.Should().Be(50m); // 100 - 50
    }

    [Fact]
    public void Reinstate_CancelledPolicy_ReinstatesPolicy()
    {
        // Arrange
        var policy = CreateCancelledPolicy(CancellationType.InsuredRequest);
        policy.ClearDomainEvents();

        // Act
        policy.Reinstate("Customer resolved billing issue");

        // Assert
        policy.Status.Should().Be(PolicyStatus.Active);
        policy.ReinstatementDate.Should().NotBeNull();
        policy.ReinstatementReason.Should().Be("Customer resolved billing issue");
        policy.CancellationDate.Should().BeNull();
        policy.CancellationReason.Should().BeNull();
        policy.CancellationType.Should().BeNull();
    }

    [Fact]
    public void Reinstate_CancelledPolicy_RaisesPolicyReinstatedEvent()
    {
        // Arrange
        var policy = CreateCancelledPolicy(CancellationType.InsuredRequest);
        policy.ClearDomainEvents();

        // Act
        policy.Reinstate("Customer resolved billing issue");

        // Assert
        policy.DomainEvents.Should().HaveCount(1);
        policy.DomainEvents.First().Should().BeOfType<PolicyReinstatedEvent>();
    }

    [Fact]
    public void Reinstate_NotCancelledPolicy_ThrowsBusinessRuleViolationException()
    {
        // Arrange
        var policy = CreateTestPolicy();
        policy.AddCoverage("BI", "Bodily Injury", Money.Create(500m));
        policy.Bind(_userId);
        policy.Activate();

        // Act
        var act = () => policy.Reinstate("Some reason");

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*Only cancelled policies*");
    }

    [Fact]
    public void Reinstate_FlatCancelPolicy_ThrowsBusinessRuleViolationException()
    {
        // Arrange
        var policy = CreateCancelledPolicy(CancellationType.FlatCancel);
        policy.ClearDomainEvents();

        // Act
        var act = () => policy.Reinstate("Trying to reinstate");

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*Flat-cancelled*");
    }

    [Fact]
    public void Reinstate_MisrepresentationPolicy_ThrowsBusinessRuleViolationException()
    {
        // Arrange
        var policy = CreateCancelledPolicy(CancellationType.Misrepresentation);
        policy.ClearDomainEvents();

        // Act
        var act = () => policy.Reinstate("Trying to reinstate");

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*misrepresentation*");
    }

    private Policy CreateTestPolicy()
    {
        var effectivePeriod = EffectivePeriod.Annual(new DateOnly(2024, 1, 1));

        return Policy.Create(
            _tenantId,
            _clientId,
            _carrierId,
            LineOfBusiness.PersonalAuto,
            "Personal Auto Policy",
            effectivePeriod,
            _userId);
    }

    private Policy CreateCancelledPolicy(CancellationType cancellationType)
    {
        var policy = CreateTestPolicy();
        policy.AddCoverage("BI", "Bodily Injury", Money.Create(500m));
        policy.Bind(_userId);
        policy.Activate();
        var cancellationDate = policy.EffectivePeriod.EffectiveDate.AddMonths(1);
        policy.Cancel(cancellationDate, "Test cancellation", cancellationType);
        return policy;
    }
}
