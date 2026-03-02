using FluentAssertions;
using IBS.BuildingBlocks.Domain;
using IBS.Carriers.Domain.ValueObjects;
using IBS.Policies.Domain.Aggregates.Quote;
using IBS.Policies.Domain.Events;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Policies.Domain.ValueObjects;

namespace IBS.UnitTests.Policies.Domain;

/// <summary>
/// Unit tests for the Quote aggregate root.
/// </summary>
public class QuoteTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _clientId = Guid.NewGuid();
    private readonly Guid _carrierId1 = Guid.NewGuid();
    private readonly Guid _carrierId2 = Guid.NewGuid();

    private Quote CreateDraftQuote()
    {
        var effectivePeriod = EffectivePeriod.Annual(new DateOnly(2025, 1, 1));
        return Quote.Create(
            _tenantId,
            _clientId,
            LineOfBusiness.GeneralLiability,
            effectivePeriod,
            _userId,
            "Test quote");
    }

    private Quote CreateSubmittedQuote()
    {
        var quote = CreateDraftQuote();
        quote.AddCarrier(_carrierId1);
        quote.AddCarrier(_carrierId2);
        quote.ClearDomainEvents();
        quote.Submit();
        return quote;
    }

    [Fact]
    public void Create_ValidInputs_CreatesQuote()
    {
        // Arrange
        var effectivePeriod = EffectivePeriod.Annual(new DateOnly(2025, 1, 1));

        // Act
        var quote = Quote.Create(
            _tenantId,
            _clientId,
            LineOfBusiness.GeneralLiability,
            effectivePeriod,
            _userId,
            "Test notes");

        // Assert
        quote.Should().NotBeNull();
        quote.TenantId.Should().Be(_tenantId);
        quote.ClientId.Should().Be(_clientId);
        quote.LineOfBusiness.Should().Be(LineOfBusiness.GeneralLiability);
        quote.Status.Should().Be(QuoteStatus.Draft);
        quote.Notes.Should().Be("Test notes");
        quote.CreatedBy.Should().Be(_userId);
        quote.DomainEvents.Should().HaveCount(1);
        quote.DomainEvents.First().Should().BeOfType<QuoteCreatedEvent>();
    }

    [Fact]
    public void Create_DefaultExpiresAt_Sets30DaysFromNow()
    {
        // Arrange & Act
        var quote = CreateDraftQuote();

        // Assert
        var expectedDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));
        quote.ExpiresAt.Should().Be(expectedDate);
    }

    [Fact]
    public void AddCarrier_DraftQuote_AddsCarrier()
    {
        // Arrange
        var quote = CreateDraftQuote();

        // Act
        var quoteCarrier = quote.AddCarrier(_carrierId1);

        // Assert
        quote.Carriers.Should().HaveCount(1);
        quoteCarrier.CarrierId.Should().Be(_carrierId1);
        quoteCarrier.Status.Should().Be(QuoteCarrierStatus.Pending);
    }

    [Fact]
    public void AddCarrier_DuplicateCarrier_Throws()
    {
        // Arrange
        var quote = CreateDraftQuote();
        quote.AddCarrier(_carrierId1);

        // Act & Assert
        var act = () => quote.AddCarrier(_carrierId1);
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*already been added*");
    }

    [Fact]
    public void AddCarrier_SubmittedQuote_Throws()
    {
        // Arrange
        var quote = CreateSubmittedQuote();

        // Act & Assert
        var act = () => quote.AddCarrier(Guid.NewGuid());
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*draft quotes*");
    }

    [Fact]
    public void RemoveCarrier_DraftQuote_RemovesCarrier()
    {
        // Arrange
        var quote = CreateDraftQuote();
        var qc = quote.AddCarrier(_carrierId1);

        // Act
        quote.RemoveCarrier(qc.Id);

        // Assert
        quote.Carriers.Should().BeEmpty();
    }

    [Fact]
    public void RemoveCarrier_NonExistentId_Throws()
    {
        // Arrange
        var quote = CreateDraftQuote();

        // Act & Assert
        var act = () => quote.RemoveCarrier(Guid.NewGuid());
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public void Submit_DraftWithCarriers_TransitionsToSubmitted()
    {
        // Arrange
        var quote = CreateDraftQuote();
        quote.AddCarrier(_carrierId1);
        quote.ClearDomainEvents();

        // Act
        quote.Submit();

        // Assert
        quote.Status.Should().Be(QuoteStatus.Submitted);
        quote.DomainEvents.Should().HaveCount(1);
        quote.DomainEvents.First().Should().BeOfType<QuoteSubmittedEvent>();
    }

    [Fact]
    public void Submit_DraftWithoutCarriers_Throws()
    {
        // Arrange
        var quote = CreateDraftQuote();

        // Act & Assert
        var act = () => quote.Submit();
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*at least one carrier*");
    }

    [Fact]
    public void Submit_AlreadySubmitted_Throws()
    {
        // Arrange
        var quote = CreateSubmittedQuote();

        // Act & Assert
        var act = () => quote.Submit();
        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void RecordQuotedResponse_SubmittedQuote_RecordsResponse()
    {
        // Arrange
        var quote = CreateSubmittedQuote();
        var carrierId = quote.Carriers.First().Id;
        quote.ClearDomainEvents();

        // Act
        quote.RecordQuotedResponse(carrierId, 5000m, "USD", "Subject to inspection");

        // Assert
        var carrier = quote.GetCarrier(carrierId)!;
        carrier.Status.Should().Be(QuoteCarrierStatus.Quoted);
        carrier.PremiumAmount.Should().Be(5000m);
        carrier.Conditions.Should().Be("Subject to inspection");
        carrier.RespondedAt.Should().NotBeNull();
        quote.DomainEvents.Should().HaveCount(1);
        quote.DomainEvents.First().Should().BeOfType<QuoteResponseReceivedEvent>();
    }

    [Fact]
    public void RecordDeclinedResponse_SubmittedQuote_RecordsDeclined()
    {
        // Arrange
        var quote = CreateSubmittedQuote();
        var carrierId = quote.Carriers.First().Id;
        quote.ClearDomainEvents();

        // Act
        quote.RecordDeclinedResponse(carrierId, "Outside appetite");

        // Assert
        var carrier = quote.GetCarrier(carrierId)!;
        carrier.Status.Should().Be(QuoteCarrierStatus.Declined);
        carrier.DeclinationReason.Should().Be("Outside appetite");
    }

    [Fact]
    public void RecordResponse_AllRespondedWithQuote_TransitionsToQuoted()
    {
        // Arrange
        var quote = CreateSubmittedQuote();
        var carriers = quote.Carriers.ToList();

        // Act
        quote.RecordQuotedResponse(carriers[0].Id, 5000m);
        quote.RecordDeclinedResponse(carriers[1].Id, "No appetite");

        // Assert
        quote.Status.Should().Be(QuoteStatus.Quoted);
    }

    [Fact]
    public void RecordResponse_DraftQuote_Throws()
    {
        // Arrange
        var quote = CreateDraftQuote();
        var qc = quote.AddCarrier(_carrierId1);

        // Act & Assert
        var act = () => quote.RecordQuotedResponse(qc.Id, 5000m);
        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void Accept_QuotedCarrier_TransitionsToAccepted()
    {
        // Arrange
        var quote = CreateSubmittedQuote();
        var carriers = quote.Carriers.ToList();
        quote.RecordQuotedResponse(carriers[0].Id, 5000m);
        quote.RecordDeclinedResponse(carriers[1].Id, "No appetite");
        quote.ClearDomainEvents();
        var policyId = Guid.NewGuid();

        // Act
        quote.Accept(carriers[0].Id, policyId);

        // Assert
        quote.Status.Should().Be(QuoteStatus.Accepted);
        quote.AcceptedCarrierId.Should().Be(carriers[0].CarrierId);
        quote.PolicyId.Should().Be(policyId);
        quote.DomainEvents.Should().HaveCount(1);
        quote.DomainEvents.First().Should().BeOfType<QuoteAcceptedEvent>();
    }

    [Fact]
    public void Accept_DeclinedCarrier_Throws()
    {
        // Arrange
        var quote = CreateSubmittedQuote();
        var carriers = quote.Carriers.ToList();
        quote.RecordQuotedResponse(carriers[0].Id, 5000m);
        quote.RecordDeclinedResponse(carriers[1].Id, "No appetite");

        // Act & Assert
        var act = () => quote.Accept(carriers[1].Id, Guid.NewGuid());
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*provided a quote*");
    }

    [Fact]
    public void Accept_ExpiredQuote_Throws()
    {
        // Arrange
        var effectivePeriod = EffectivePeriod.Annual(new DateOnly(2025, 1, 1));
        var quote = Quote.Create(
            _tenantId,
            _clientId,
            LineOfBusiness.GeneralLiability,
            effectivePeriod,
            _userId,
            expiresAt: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)));
        quote.AddCarrier(_carrierId1);
        quote.Submit();
        quote.RecordQuotedResponse(quote.Carriers.First().Id, 5000m);

        // Act & Assert
        var act = () => quote.Accept(quote.Carriers.First().Id, Guid.NewGuid());
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*expired*");
    }

    [Fact]
    public void Cancel_DraftQuote_TransitionsToCancelled()
    {
        // Arrange
        var quote = CreateDraftQuote();
        quote.ClearDomainEvents();

        // Act
        quote.Cancel();

        // Assert
        quote.Status.Should().Be(QuoteStatus.Cancelled);
        quote.DomainEvents.Should().HaveCount(1);
        quote.DomainEvents.First().Should().BeOfType<QuoteCancelledEvent>();
    }

    [Fact]
    public void Cancel_SubmittedQuote_TransitionsToCancelled()
    {
        // Arrange
        var quote = CreateSubmittedQuote();

        // Act
        quote.Cancel();

        // Assert
        quote.Status.Should().Be(QuoteStatus.Cancelled);
    }

    [Fact]
    public void Cancel_AcceptedQuote_Throws()
    {
        // Arrange
        var quote = CreateSubmittedQuote();
        var carriers = quote.Carriers.ToList();
        quote.RecordQuotedResponse(carriers[0].Id, 5000m);
        quote.RecordDeclinedResponse(carriers[1].Id);
        quote.Accept(carriers[0].Id, Guid.NewGuid());

        // Act & Assert
        var act = () => quote.Cancel();
        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void Expire_DraftQuote_TransitionsToExpired()
    {
        // Arrange
        var quote = CreateDraftQuote();
        quote.ClearDomainEvents();

        // Act
        quote.Expire();

        // Assert
        quote.Status.Should().Be(QuoteStatus.Expired);
        quote.DomainEvents.Should().HaveCount(1);
        quote.DomainEvents.First().Should().BeOfType<QuoteExpiredEvent>();
    }

    [Fact]
    public void Expire_AlreadyExpired_Throws()
    {
        // Arrange
        var quote = CreateDraftQuote();
        quote.Expire();

        // Act & Assert
        var act = () => quote.Expire();
        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void GetCarrier_ExistingId_ReturnsCarrier()
    {
        // Arrange
        var quote = CreateDraftQuote();
        var qc = quote.AddCarrier(_carrierId1);

        // Act
        var found = quote.GetCarrier(qc.Id);

        // Assert
        found.Should().NotBeNull();
        found!.CarrierId.Should().Be(_carrierId1);
    }

    [Fact]
    public void GetCarrier_NonExistentId_ReturnsNull()
    {
        // Arrange
        var quote = CreateDraftQuote();

        // Act
        var found = quote.GetCarrier(Guid.NewGuid());

        // Assert
        found.Should().BeNull();
    }
}
