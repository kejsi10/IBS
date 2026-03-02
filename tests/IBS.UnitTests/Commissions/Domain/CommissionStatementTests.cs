using FluentAssertions;
using IBS.BuildingBlocks.Domain;
using IBS.Commissions.Domain.Aggregates.CommissionStatement;
using IBS.Commissions.Domain.Events;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Commissions.Domain.ValueObjects;

namespace IBS.UnitTests.Commissions.Domain;

/// <summary>
/// Unit tests for the CommissionStatement aggregate root.
/// </summary>
public class CommissionStatementTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _carrierId = Guid.NewGuid();

    private CommissionStatement CreateTestStatement()
    {
        return CommissionStatement.Create(
            _tenantId,
            _carrierId,
            "Acme Insurance",
            "STMT-2026-001",
            1,
            2026,
            new DateOnly(2026, 1, 31),
            Money.Usd(100000m),
            Money.Usd(15000m));
    }

    private CommissionLineItem AddTestLineItem(CommissionStatement statement)
    {
        return statement.AddLineItem(
            "POL-001",
            "John's Bakery LLC",
            "Commercial Property",
            new DateOnly(2026, 1, 1),
            TransactionType.NewBusiness,
            Money.Usd(10000m),
            15m,
            Money.Usd(1500m));
    }

    [Fact]
    public void Create_ValidInputs_CreatesStatement()
    {
        // Arrange & Act
        var statement = CreateTestStatement();

        // Assert
        statement.Should().NotBeNull();
        statement.TenantId.Should().Be(_tenantId);
        statement.CarrierId.Should().Be(_carrierId);
        statement.CarrierName.Should().Be("Acme Insurance");
        statement.StatementNumber.Should().Be("STMT-2026-001");
        statement.PeriodMonth.Should().Be(1);
        statement.PeriodYear.Should().Be(2026);
        statement.Status.Should().Be(StatementStatus.Received);
        statement.TotalPremium.Amount.Should().Be(100000m);
        statement.TotalCommission.Amount.Should().Be(15000m);
    }

    [Fact]
    public void Create_ValidInputs_RaisesStatementReceivedEvent()
    {
        // Arrange & Act
        var statement = CreateTestStatement();

        // Assert
        statement.DomainEvents.Should().HaveCount(1);
        var evt = statement.DomainEvents.First().Should().BeOfType<StatementReceivedEvent>().Subject;
        evt.CarrierId.Should().Be(_carrierId);
        evt.StatementNumber.Should().Be("STMT-2026-001");
    }

    [Fact]
    public void Create_InvalidMonth_Throws()
    {
        // Arrange & Act
        var act = () => CommissionStatement.Create(
            _tenantId, _carrierId, "Carrier", "STMT-1", 13, 2026,
            new DateOnly(2026, 1, 1), Money.Usd(1000m), Money.Usd(100m));

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*month*1*12*");
    }

    [Fact]
    public void AddLineItem_ValidInputs_AddsLineItem()
    {
        // Arrange
        var statement = CreateTestStatement();

        // Act
        var lineItem = AddTestLineItem(statement);

        // Assert
        statement.LineItems.Should().HaveCount(1);
        lineItem.PolicyNumber.Should().Be("POL-001");
        lineItem.InsuredName.Should().Be("John's Bakery LLC");
        lineItem.TransactionType.Should().Be(TransactionType.NewBusiness);
        lineItem.GrossPremium.Amount.Should().Be(10000m);
        lineItem.CommissionRate.Should().Be(15m);
        lineItem.CommissionAmount.Amount.Should().Be(1500m);
        lineItem.IsReconciled.Should().BeFalse();
    }

    [Fact]
    public void ReconcileLineItem_ValidLineItem_ReconcilesIt()
    {
        // Arrange
        var statement = CreateTestStatement();
        var lineItem = AddTestLineItem(statement);
        statement.ClearDomainEvents();

        // Act
        statement.ReconcileLineItem(lineItem.Id);

        // Assert
        lineItem.IsReconciled.Should().BeTrue();
        lineItem.ReconciledAt.Should().NotBeNull();
        statement.DomainEvents.Should().ContainSingle(e => e is LineItemReconciledEvent);
    }

    [Fact]
    public void ReconcileLineItem_AlreadyReconciled_Throws()
    {
        // Arrange
        var statement = CreateTestStatement();
        var lineItem = AddTestLineItem(statement);
        statement.ReconcileLineItem(lineItem.Id);

        // Act
        var act = () => statement.ReconcileLineItem(lineItem.Id);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*already reconciled*");
    }

    [Fact]
    public void DisputeLineItem_ValidLineItem_DisputesIt()
    {
        // Arrange
        var statement = CreateTestStatement();
        var lineItem = AddTestLineItem(statement);
        statement.ClearDomainEvents();

        // Act
        statement.DisputeLineItem(lineItem.Id, "Rate discrepancy");

        // Assert
        lineItem.IsReconciled.Should().BeFalse();
        lineItem.DisputeReason.Should().Be("Rate discrepancy");
        statement.DomainEvents.Should().ContainSingle(e => e is LineItemDisputedEvent);
    }

    [Fact]
    public void AddProducerSplit_ValidInputs_AddsSplit()
    {
        // Arrange
        var statement = CreateTestStatement();
        var lineItem = AddTestLineItem(statement);
        var producerId = Guid.NewGuid();
        statement.ClearDomainEvents();

        // Act
        var split = statement.AddProducerSplit(
            lineItem.Id, "Jane Agent", producerId, 60m, Money.Usd(900m));

        // Assert
        statement.ProducerSplits.Should().HaveCount(1);
        split.ProducerName.Should().Be("Jane Agent");
        split.ProducerId.Should().Be(producerId);
        split.SplitPercentage.Should().Be(60m);
        split.SplitAmount.Amount.Should().Be(900m);
        statement.DomainEvents.Should().ContainSingle(e => e is ProducerCreditAssignedEvent);
    }

    [Fact]
    public void AddProducerSplit_ExceedsTotalPercentage_Throws()
    {
        // Arrange
        var statement = CreateTestStatement();
        var lineItem = AddTestLineItem(statement);
        statement.AddProducerSplit(lineItem.Id, "Agent A", Guid.NewGuid(), 60m, Money.Usd(900m));

        // Act
        var act = () => statement.AddProducerSplit(
            lineItem.Id, "Agent B", Guid.NewGuid(), 50m, Money.Usd(750m));

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*exceed 100%*");
    }

    [Fact]
    public void StartReconciling_FromReceived_TransitionsToReconciling()
    {
        // Arrange
        var statement = CreateTestStatement();

        // Act
        statement.StartReconciling();

        // Assert
        statement.Status.Should().Be(StatementStatus.Reconciling);
    }

    [Fact]
    public void MarkReconciled_AllItemsReconciled_TransitionsToReconciled()
    {
        // Arrange
        var statement = CreateTestStatement();
        var lineItem = AddTestLineItem(statement);
        statement.StartReconciling();
        statement.ReconcileLineItem(lineItem.Id);
        statement.ClearDomainEvents();

        // Act
        statement.MarkReconciled();

        // Assert
        statement.Status.Should().Be(StatementStatus.Reconciled);
        statement.DomainEvents.Should().ContainSingle(e => e is StatementReconciledEvent);
    }

    [Fact]
    public void MarkReconciled_UnreconciledItems_Throws()
    {
        // Arrange
        var statement = CreateTestStatement();
        AddTestLineItem(statement);
        statement.StartReconciling();

        // Act
        var act = () => statement.MarkReconciled();

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*neither reconciled nor disputed*");
    }

    [Fact]
    public void MarkReconciled_DisputedItemsAllowed_TransitionsToReconciled()
    {
        // Arrange
        var statement = CreateTestStatement();
        var lineItem = AddTestLineItem(statement);
        statement.StartReconciling();
        statement.DisputeLineItem(lineItem.Id, "Wrong rate");

        // Act
        statement.MarkReconciled();

        // Assert
        statement.Status.Should().Be(StatementStatus.Reconciled);
    }

    [Fact]
    public void MarkPaid_FromReconciled_TransitionsToPaid()
    {
        // Arrange
        var statement = CreateTestStatement();
        var lineItem = AddTestLineItem(statement);
        statement.StartReconciling();
        statement.ReconcileLineItem(lineItem.Id);
        statement.MarkReconciled();
        statement.ClearDomainEvents();

        // Act
        statement.MarkPaid();

        // Assert
        statement.Status.Should().Be(StatementStatus.Paid);
        statement.DomainEvents.Should().ContainSingle(e => e is StatementPaidEvent);
    }

    [Fact]
    public void MarkPaid_FromReceived_Throws()
    {
        // Arrange
        var statement = CreateTestStatement();

        // Act
        var act = () => statement.MarkPaid();

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*Cannot transition*");
    }

    [Fact]
    public void Reopen_FromDisputed_TransitionsToReconciling()
    {
        // Arrange
        var statement = CreateTestStatement();
        statement.StartReconciling();
        statement.MarkDisputed();

        // Act
        statement.Reopen();

        // Assert
        statement.Status.Should().Be(StatementStatus.Reconciling);
    }

    [Fact]
    public void Reopen_FromNonDisputed_Throws()
    {
        // Arrange
        var statement = CreateTestStatement();

        // Act
        var act = () => statement.Reopen();

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*Only disputed*");
    }

    [Fact]
    public void MarkReconciled_NoLineItems_Throws()
    {
        // Arrange
        var statement = CreateTestStatement();
        statement.StartReconciling();

        // Act
        var act = () => statement.MarkReconciled();

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*no line items*");
    }
}
