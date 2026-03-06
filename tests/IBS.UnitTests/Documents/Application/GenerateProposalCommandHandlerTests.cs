using FluentAssertions;
using IBS.BuildingBlocks.Domain;
using IBS.Documents.Application.Commands.GenerateProposal;
using IBS.Documents.Application.Services;
using IBS.Documents.Domain.Aggregates.Document;
using IBS.Documents.Domain.Aggregates.DocumentTemplate;
using IBS.Documents.Domain.Repositories;
using NSubstitute;

namespace IBS.UnitTests.Documents.Application;

/// <summary>
/// Unit tests for the GenerateProposalCommandHandler.
/// </summary>
public class GenerateProposalCommandHandlerTests
{
    private readonly IDocumentTemplateRepository _templateRepository = Substitute.For<IDocumentTemplateRepository>();
    private readonly IDocumentRepository _documentRepository = Substitute.For<IDocumentRepository>();
    private readonly IQuoteDataService _quoteDataService = Substitute.For<IQuoteDataService>();
    private readonly IPdfGeneratorService _pdfGeneratorService = Substitute.For<IPdfGeneratorService>();
    private readonly IBlobStorageService _blobStorageService = Substitute.For<IBlobStorageService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly GenerateProposalCommandHandler _handler;

    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _templateId = Guid.NewGuid();
    private readonly Guid _quoteId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="GenerateProposalCommandHandlerTests"/> class.
    /// </summary>
    public GenerateProposalCommandHandlerTests()
    {
        _handler = new GenerateProposalCommandHandler(
            _templateRepository,
            _documentRepository,
            _quoteDataService,
            _pdfGeneratorService,
            _blobStorageService,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_ValidCommand_GeneratesPdfAndReturnsDocumentId()
    {
        // Arrange
        var template = CreateTemplate();
        var quoteData = CreateQuoteProposalData();
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };

        _templateRepository.GetByIdAsync(_templateId, Arg.Any<CancellationToken>()).Returns(template);
        _quoteDataService.GetQuoteProposalDataAsync(_quoteId, Arg.Any<CancellationToken>()).Returns(quoteData);
        _pdfGeneratorService.GenerateAsync(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>()).Returns(pdfBytes);

        var command = new GenerateProposalCommand(_tenantId, "user@test.com", _templateId, _quoteId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        await _documentRepository.Received(1).AddAsync(Arg.Any<Document>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_TemplateNotFound_ReturnsNotFoundError()
    {
        // Arrange
        _templateRepository.GetByIdAsync(_templateId, Arg.Any<CancellationToken>()).Returns((DocumentTemplate?)null);

        var command = new GenerateProposalCommand(_tenantId, "user@test.com", _templateId, _quoteId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("NotFound");
    }

    [Fact]
    public async Task Handle_TemplateBelongsToDifferentTenant_ReturnsNotFoundError()
    {
        // Arrange
        var otherTenantId = Guid.NewGuid();
        var template = DocumentTemplate.Create(
            otherTenantId, "Proposal", "desc", TemplateType.Proposal, "<html></html>", "system");

        _templateRepository.GetByIdAsync(_templateId, Arg.Any<CancellationToken>()).Returns(template);

        var command = new GenerateProposalCommand(_tenantId, "user@test.com", _templateId, _quoteId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("NotFound");
    }

    [Fact]
    public async Task Handle_QuoteNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var template = CreateTemplate();
        _templateRepository.GetByIdAsync(_templateId, Arg.Any<CancellationToken>()).Returns(template);
        _quoteDataService.GetQuoteProposalDataAsync(_quoteId, Arg.Any<CancellationToken>()).Returns((QuoteProposalData?)null);

        var command = new GenerateProposalCommand(_tenantId, "user@test.com", _templateId, _quoteId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("NotFound");
    }

    [Fact]
    public async Task Handle_PdfGenerationFails_ReturnsInternalError()
    {
        // Arrange
        var template = CreateTemplate();
        var quoteData = CreateQuoteProposalData();
        _templateRepository.GetByIdAsync(_templateId, Arg.Any<CancellationToken>()).Returns(template);
        _quoteDataService.GetQuoteProposalDataAsync(_quoteId, Arg.Any<CancellationToken>()).Returns(quoteData);
        _pdfGeneratorService.GenerateAsync(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns<byte[]>(_ => throw new InvalidOperationException("Browser crashed"));

        var command = new GenerateProposalCommand(_tenantId, "user@test.com", _templateId, _quoteId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Internal");
        result.Error.Message.Should().Contain("Browser crashed");
    }

    private DocumentTemplate CreateTemplate() =>
        DocumentTemplate.Create(_tenantId, "Standard Proposal", "desc", TemplateType.Proposal, "<html>{{ClientName}}</html>", "system");

    private static QuoteProposalData CreateQuoteProposalData() => new()
    {
        ClientName = "Acme Corp",
        ClientAddress = "123 Main St, Springfield",
        LineOfBusiness = "GeneralLiability",
        EffectiveDate = new DateOnly(2024, 1, 1),
        ExpirationDate = new DateOnly(2025, 1, 1),
        Notes = "Renewal proposal",
        CarrierOffers =
        [
            new QuoteCarrierProposalData
            {
                CarrierName = "Safe Insurance Co",
                Status = "Quoted",
                PremiumAmount = 10000m,
                Conditions = "No prior claims",
                ProposedCoverages = "GL: $1M/$2M"
            }
        ]
    };
}
