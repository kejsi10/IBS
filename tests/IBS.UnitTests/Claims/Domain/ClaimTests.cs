using FluentAssertions;
using IBS.BuildingBlocks.Domain;
using IBS.Claims.Domain.Aggregates.Claim;
using IBS.Claims.Domain.Events;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Claims.Domain.ValueObjects;

namespace IBS.UnitTests.Claims.Domain;

/// <summary>
/// Unit tests for the Claim aggregate root.
/// </summary>
public class ClaimTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _policyId = Guid.NewGuid();
    private readonly Guid _clientId = Guid.NewGuid();

    private Claim CreateTestClaim(Money? estimatedLoss = null)
    {
        return Claim.Create(
            _tenantId,
            _policyId,
            _clientId,
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow,
            LossType.PropertyDamage,
            "Water damage from burst pipe in office building.",
            _userId,
            estimatedLoss);
    }

    [Fact]
    public void Create_ValidInputs_CreatesClaim()
    {
        // Arrange & Act
        var claim = CreateTestClaim();

        // Assert
        claim.Should().NotBeNull();
        claim.TenantId.Should().Be(_tenantId);
        claim.PolicyId.Should().Be(_policyId);
        claim.ClientId.Should().Be(_clientId);
        claim.Status.Should().Be(ClaimStatus.FNOL);
        claim.LossType.Should().Be(LossType.PropertyDamage);
        claim.LossDescription.Should().Be("Water damage from burst pipe in office building.");
        claim.CreatedBy.Should().Be(_userId);
        claim.ClaimNumber.Value.Should().StartWith("CLM-");
        claim.DomainEvents.Should().HaveCount(1);
        claim.DomainEvents.First().Should().BeOfType<ClaimCreatedEvent>();
    }

    [Fact]
    public void Create_WithEstimatedLoss_SetsLossAmount()
    {
        // Arrange & Act
        var claim = CreateTestClaim(Money.Usd(50000));

        // Assert
        claim.LossAmount.Should().NotBeNull();
        claim.LossAmount!.Amount.Should().Be(50000);
        claim.LossAmount.Currency.Should().Be("USD");
    }

    [Fact]
    public void Create_LossDateAfterReportedDate_Throws()
    {
        // Arrange
        var lossDate = DateTimeOffset.UtcNow.AddDays(1);
        var reportedDate = DateTimeOffset.UtcNow;

        // Act
        var act = () => Claim.Create(
            _tenantId, _policyId, _clientId,
            lossDate, reportedDate,
            LossType.Auto, "Test", _userId);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*Loss date*after*reported date*");
    }

    [Fact]
    public void Create_EmptyDescription_Throws()
    {
        // Act
        var act = () => Claim.Create(
            _tenantId, _policyId, _clientId,
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow,
            LossType.Auto, "", _userId);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Acknowledge_FromFNOL_TransitionsToAcknowledged()
    {
        // Arrange
        var claim = CreateTestClaim();

        // Act
        claim.Acknowledge();

        // Assert
        claim.Status.Should().Be(ClaimStatus.Acknowledged);
        claim.DomainEvents.Should().Contain(e => e is ClaimAcknowledgedEvent);
    }

    [Fact]
    public void AssignAdjuster_FromAcknowledged_TransitionsToAssigned()
    {
        // Arrange
        var claim = CreateTestClaim();
        claim.Acknowledge();
        claim.ClearDomainEvents();

        // Act
        claim.AssignAdjuster("ADJ-001");

        // Assert
        claim.Status.Should().Be(ClaimStatus.Assigned);
        claim.AssignedAdjusterId.Should().Be("ADJ-001");
        claim.DomainEvents.Should().Contain(e => e is AdjusterAssignedEvent);
    }

    [Fact]
    public void StartInvestigation_FromAssigned_TransitionsToUnderInvestigation()
    {
        // Arrange
        var claim = CreateTestClaim();
        claim.Acknowledge();
        claim.AssignAdjuster("ADJ-001");

        // Act
        claim.StartInvestigation();

        // Assert
        claim.Status.Should().Be(ClaimStatus.UnderInvestigation);
    }

    [Fact]
    public void Approve_FromEvaluation_TransitionsToApproved()
    {
        // Arrange
        var claim = CreateTestClaim();
        claim.Acknowledge();
        claim.AssignAdjuster("ADJ-001");
        claim.StartInvestigation();
        claim.Evaluate();

        // Act
        claim.Approve(Money.Usd(25000));

        // Assert
        claim.Status.Should().Be(ClaimStatus.Approved);
        claim.ClaimAmount.Should().NotBeNull();
        claim.ClaimAmount!.Amount.Should().Be(25000);
        claim.DomainEvents.Should().Contain(e => e is ClaimApprovedEvent);
    }

    [Fact]
    public void Deny_FromEvaluation_TransitionsToDenied()
    {
        // Arrange
        var claim = CreateTestClaim();
        claim.Acknowledge();
        claim.AssignAdjuster("ADJ-001");
        claim.StartInvestigation();
        claim.Evaluate();

        // Act
        claim.Deny("Pre-existing condition, not covered under policy.");

        // Assert
        claim.Status.Should().Be(ClaimStatus.Denied);
        claim.DenialReason.Should().Be("Pre-existing condition, not covered under policy.");
        claim.DomainEvents.Should().Contain(e => e is ClaimDeniedEvent);
    }

    [Fact]
    public void Close_FromSettlement_TransitionsToClosed()
    {
        // Arrange
        var claim = CreateTestClaim();
        claim.Acknowledge();
        claim.AssignAdjuster("ADJ-001");
        claim.StartInvestigation();
        claim.Evaluate();
        claim.Approve(Money.Usd(25000));
        claim.MoveToSettlement();

        // Act
        claim.Close("Settlement completed, all payments issued.");

        // Assert
        claim.Status.Should().Be(ClaimStatus.Closed);
        claim.ClosedAt.Should().NotBeNull();
        claim.ClosureReason.Should().Be("Settlement completed, all payments issued.");
        claim.DomainEvents.Should().Contain(e => e is ClaimClosedEvent);
    }

    [Fact]
    public void Close_WithPendingPayments_Throws()
    {
        // Arrange
        var claim = CreateTestClaim();
        claim.Acknowledge();
        claim.AssignAdjuster("ADJ-001");
        claim.StartInvestigation();
        claim.Evaluate();
        claim.Approve(Money.Usd(25000));
        claim.MoveToSettlement();
        claim.AuthorizePayment("Indemnity", Money.Usd(10000), "John Doe", DateOnly.FromDateTime(DateTime.Today), _userId.ToString());

        // Act
        var act = () => claim.Close("Done");

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*pending payments*");
    }

    [Fact]
    public void Reopen_FromClosed_TransitionsToUnderInvestigation()
    {
        // Arrange
        var claim = CreateTestClaim();
        claim.Acknowledge();
        claim.AssignAdjuster("ADJ-001");
        claim.StartInvestigation();
        claim.Evaluate();
        claim.Deny("Initially denied");
        claim.Close("Denied claim closure");

        // Act
        claim.Reopen();

        // Assert
        claim.Status.Should().Be(ClaimStatus.UnderInvestigation);
        claim.ClosedAt.Should().BeNull();
        claim.ClosureReason.Should().BeNull();
        claim.DomainEvents.Should().Contain(e => e is ClaimReopenedEvent);
    }

    [Fact]
    public void InvalidTransition_Throws()
    {
        // Arrange
        var claim = CreateTestClaim();

        // Act — skip acknowledgement, try to assign
        var act = () => claim.AssignAdjuster("ADJ-001");

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*Cannot transition*");
    }

    [Fact]
    public void AddNote_CreatesNote()
    {
        // Arrange
        var claim = CreateTestClaim();

        // Act
        var note = claim.AddNote("Initial assessment completed.", _userId.ToString(), isInternal: true);

        // Assert
        note.Should().NotBeNull();
        note.Content.Should().Be("Initial assessment completed.");
        note.IsInternal.Should().BeTrue();
        claim.Notes.Should().HaveCount(1);
    }

    [Fact]
    public void SetReserve_CreatesReserve()
    {
        // Arrange
        var claim = CreateTestClaim();

        // Act
        var reserve = claim.SetReserve("Indemnity", Money.Usd(50000), _userId.ToString(), "Initial reserve");

        // Assert
        reserve.Should().NotBeNull();
        reserve.ReserveType.Should().Be("Indemnity");
        reserve.Amount.Amount.Should().Be(50000);
        claim.Reserves.Should().HaveCount(1);
        claim.GetTotalReserves().Should().Be(50000);
        claim.DomainEvents.Should().Contain(e => e is ReserveSetEvent);
    }

    [Fact]
    public void AuthorizePayment_CreatesPayment()
    {
        // Arrange
        var claim = CreateTestClaim();
        claim.Acknowledge();
        claim.AssignAdjuster("ADJ-001");
        claim.StartInvestigation();
        claim.Evaluate();
        claim.Approve(Money.Usd(25000));

        // Act
        var payment = claim.AuthorizePayment(
            "Indemnity", Money.Usd(10000), "John Doe",
            DateOnly.FromDateTime(DateTime.Today), _userId.ToString(), "CHK-001");

        // Assert
        payment.Should().NotBeNull();
        payment.PaymentType.Should().Be("Indemnity");
        payment.Amount.Amount.Should().Be(10000);
        payment.Status.Should().Be(PaymentStatus.Authorized);
        payment.CheckNumber.Should().Be("CHK-001");
        claim.Payments.Should().HaveCount(1);
        claim.DomainEvents.Should().Contain(e => e is PaymentAuthorizedEvent);
    }

    [Fact]
    public void AuthorizePayment_ExceedsClaimAmount_Throws()
    {
        // Arrange
        var claim = CreateTestClaim();
        claim.Acknowledge();
        claim.AssignAdjuster("ADJ-001");
        claim.StartInvestigation();
        claim.Evaluate();
        claim.Approve(Money.Usd(10000));

        // Act
        var act = () => claim.AuthorizePayment(
            "Indemnity", Money.Usd(15000), "John Doe",
            DateOnly.FromDateTime(DateTime.Today), _userId.ToString());

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*exceed*approved*");
    }

    [Fact]
    public void AuthorizePayment_OnDeniedClaim_Throws()
    {
        // Arrange
        var claim = CreateTestClaim();
        claim.Acknowledge();
        claim.AssignAdjuster("ADJ-001");
        claim.StartInvestigation();
        claim.Evaluate();
        claim.Deny("Not covered");

        // Act
        var act = () => claim.AuthorizePayment(
            "Indemnity", Money.Usd(5000), "John Doe",
            DateOnly.FromDateTime(DateTime.Today), _userId.ToString());

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*denied*");
    }

    [Fact]
    public void IssuePayment_AuthorizedPayment_IssuedSuccessfully()
    {
        // Arrange
        var claim = CreateTestClaim();
        claim.Acknowledge();
        claim.AssignAdjuster("ADJ-001");
        claim.StartInvestigation();
        claim.Evaluate();
        claim.Approve(Money.Usd(25000));
        var payment = claim.AuthorizePayment(
            "Indemnity", Money.Usd(10000), "John Doe",
            DateOnly.FromDateTime(DateTime.Today), _userId.ToString());

        // Act
        claim.IssuePayment(payment.Id);

        // Assert
        var issuedPayment = claim.Payments.First();
        issuedPayment.Status.Should().Be(PaymentStatus.Issued);
        issuedPayment.IssuedAt.Should().NotBeNull();
        claim.DomainEvents.Should().Contain(e => e is PaymentIssuedEvent);
    }

    [Fact]
    public void VoidPayment_AuthorizedPayment_VoidedSuccessfully()
    {
        // Arrange
        var claim = CreateTestClaim();
        claim.Acknowledge();
        claim.AssignAdjuster("ADJ-001");
        claim.StartInvestigation();
        claim.Evaluate();
        claim.Approve(Money.Usd(25000));
        var payment = claim.AuthorizePayment(
            "Indemnity", Money.Usd(10000), "John Doe",
            DateOnly.FromDateTime(DateTime.Today), _userId.ToString());

        // Act
        claim.VoidPayment(payment.Id, "Incorrect payee information.");

        // Assert
        var voidedPayment = claim.Payments.First();
        voidedPayment.Status.Should().Be(PaymentStatus.Voided);
        voidedPayment.VoidReason.Should().Be("Incorrect payee information.");
        voidedPayment.VoidedAt.Should().NotBeNull();
    }

    [Fact]
    public void GetTotalActivePayments_ExcludesVoidedPayments()
    {
        // Arrange
        var claim = CreateTestClaim();
        claim.Acknowledge();
        claim.AssignAdjuster("ADJ-001");
        claim.StartInvestigation();
        claim.Evaluate();
        claim.Approve(Money.Usd(50000));

        var payment1 = claim.AuthorizePayment("Indemnity", Money.Usd(10000), "John", DateOnly.FromDateTime(DateTime.Today), _userId.ToString());
        claim.AuthorizePayment("Expense", Money.Usd(5000), "Jane", DateOnly.FromDateTime(DateTime.Today), _userId.ToString());
        claim.VoidPayment(payment1.Id, "Cancelled");

        // Act
        var total = claim.GetTotalActivePayments();

        // Assert
        total.Should().Be(5000);
    }
}
