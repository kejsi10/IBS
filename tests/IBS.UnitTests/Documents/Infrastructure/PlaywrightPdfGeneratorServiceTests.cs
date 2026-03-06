using FluentAssertions;
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
        capturedHtml.Should().Contain("POL-001");
        capturedHtml.Should().Contain("Acme Corp");
        capturedHtml.Should().NotContain("{{PolicyNumber}}");
        capturedHtml.Should().NotContain("{{ClientName}}");
    }

    [Fact]
    public async Task GenerateAsync_RendersPreFormattedDatesFromCaller()
    {
        // Arrange — caller is responsible for pre-formatting dates before passing to GenerateAsync
        const string template = "<html><body><p>{{EffectiveDate}} to {{ExpirationDate}}</p></body></html>";
        var data = new { EffectiveDate = "01/01/2024", ExpirationDate = "01/01/2025" };

        string capturedHtml = string.Empty;
        await _page.SetContentAsync(
            Arg.Do<string>(html => capturedHtml = html),
            Arg.Any<PageSetContentOptions?>());

        // Act
        await _sut.GenerateAsync(template, data);

        // Assert
        capturedHtml.Should().Contain("01/01/2024");
        capturedHtml.Should().Contain("01/01/2025");
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

    private static object CreateSampleData() => new
    {
        PolicyNumber = "POL-001",
        ClientName = "Acme Corp",
        CarrierName = "Safe Insurance Co",
        EffectiveDate = "01/01/2024",
        ExpirationDate = "01/01/2025",
        LineOfBusiness = "GeneralLiability",
        CoverageSummary = new[] { "General Liability: $1M/$2M", "Commercial Property: $500K" },
        BrokerName = "IBS Brokerage",
        IssuedDate = "06/01/2024"
    };
}
