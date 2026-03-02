using FluentAssertions;
using IBS.Documents.Application.Services;
using IBS.Documents.Infrastructure.Pdf;
using Microsoft.Playwright;
using NSubstitute;

namespace IBS.UnitTests.Documents.Infrastructure;

/// <summary>
/// Unit tests for PlaywrightPdfGeneratorService.
/// The browser manager and page are mocked so tests run without a real browser.
/// </summary>
public class PlaywrightPdfGeneratorServiceTests
{
    // A minimal fake PDF header returned by the mock page
    private static readonly byte[] FakePdfBytes = [0x25, 0x50, 0x44, 0x46, 0x2D]; // %PDF-

    private readonly IPlaywrightBrowserManager _browserManager = Substitute.For<IPlaywrightBrowserManager>();
    private readonly IBrowser _browser = Substitute.For<IBrowser>();
    private readonly IPage _page = Substitute.For<IPage>();
    private readonly PlaywrightPdfGeneratorService _sut;

    public PlaywrightPdfGeneratorServiceTests()
    {
        _browserManager.GetBrowserAsync().Returns(_browser);
        _browser.NewPageAsync(Arg.Any<BrowserNewPageOptions?>()).Returns(_page);
        _page.SetContentAsync(Arg.Any<string>(), Arg.Any<PageSetContentOptions?>()).Returns(Task.CompletedTask);
        _page.PdfAsync(Arg.Any<PagePdfOptions?>()).Returns(Task.FromResult(FakePdfBytes));
        _page.CloseAsync(Arg.Any<PageCloseOptions?>()).Returns(Task.CompletedTask);

        _sut = new PlaywrightPdfGeneratorService(_browserManager);
    }

    [Fact]
    public async Task GenerateAsync_WithValidTemplate_ReturnsPdfBytes()
    {
        // Arrange
        const string template = "<html><body><p>Policy: {{PolicyNumber}}</p></body></html>";
        var data = CreateSampleData();

        // Act
        var result = await _sut.GenerateAsync(template, data);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().StartWith([0x25, 0x50, 0x44, 0x46]); // %PDF
    }

    [Fact]
    public async Task GenerateAsync_RendersHandlebarsPlaceholders()
    {
        // Arrange
        const string template = "<html><body><p>{{PolicyNumber}} - {{ClientName}}</p></body></html>";
        var data = CreateSampleData();

        string capturedHtml = string.Empty;
        await _page.SetContentAsync(
            Arg.Do<string>(html => capturedHtml = html),
            Arg.Any<PageSetContentOptions?>());

        // Act
        await _sut.GenerateAsync(template, data);

        // Assert
        capturedHtml.Should().Contain(data.PolicyNumber);
        capturedHtml.Should().Contain(data.ClientName);
        capturedHtml.Should().NotContain("{{PolicyNumber}}");
        capturedHtml.Should().NotContain("{{ClientName}}");
    }

    [Fact]
    public async Task GenerateAsync_FormatsDatePlaceholders()
    {
        // Arrange
        const string template = "<html><body><p>{{EffectiveDate}} to {{ExpirationDate}}</p></body></html>";
        var data = CreateSampleData();

        string capturedHtml = string.Empty;
        await _page.SetContentAsync(
            Arg.Do<string>(html => capturedHtml = html),
            Arg.Any<PageSetContentOptions?>());

        // Act
        await _sut.GenerateAsync(template, data);

        // Assert
        capturedHtml.Should().Contain("01/01/2024"); // EffectiveDate formatted
        capturedHtml.Should().Contain("01/01/2025"); // ExpirationDate formatted
    }

    [Fact]
    public async Task GenerateAsync_ClosesPageAfterGeneration()
    {
        // Arrange
        var data = CreateSampleData();

        // Act
        await _sut.GenerateAsync("<html><body></body></html>", data);

        // Assert
        await _page.Received(1).CloseAsync(Arg.Any<PageCloseOptions?>());
    }

    [Fact]
    public async Task GenerateAsync_ClosesPageEvenOnPdfError()
    {
        // Arrange
        _page.PdfAsync(Arg.Any<PagePdfOptions?>()).Returns<byte[]>(_ => throw new InvalidOperationException("PDF failed"));
        var data = CreateSampleData();

        // Act
        var act = () => _sut.GenerateAsync("<html><body></body></html>", data);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        await _page.Received(1).CloseAsync(Arg.Any<PageCloseOptions?>());
    }

    private static COITemplateData CreateSampleData() => new()
    {
        PolicyNumber = "POL-001",
        ClientName = "Acme Corp",
        CarrierName = "Safe Insurance Co",
        EffectiveDate = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
        ExpirationDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
        LineOfBusiness = "GeneralLiability",
        CoverageSummary = ["General Liability: $1M/$2M", "Commercial Property: $500K"],
        BrokerName = "IBS Brokerage",
        IssuedDate = new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero)
    };
}
