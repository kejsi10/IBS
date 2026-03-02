using FluentAssertions;
using IBS.Documents.Application.Services;
using IBS.Documents.Infrastructure.Ai;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace IBS.UnitTests.Documents.Infrastructure;

/// <summary>
/// Unit tests for TemplateImportService.
/// IOllamaClient and IPdfTextExtractor are mocked to isolate the service logic.
/// </summary>
public class TemplateImportServiceTests
{
    private static readonly byte[] FakePdfBytes = [0x25, 0x50, 0x44, 0x46]; // %PDF
    private const string FakeExtractedText = "CERTIFICATE OF INSURANCE\nPolicy: 12345\n---\nPage 2 coverage details";

    private readonly IOllamaClient _ollamaClient = Substitute.For<IOllamaClient>();
    private readonly IPdfTextExtractor _pdfTextExtractor = Substitute.For<IPdfTextExtractor>();
    private readonly TemplateImportService _sut;

    public TemplateImportServiceTests()
    {
        var options = Options.Create(new OllamaOptions
        {
            CoderModel = "qwen2.5-coder:7b"
        });

        _sut = new TemplateImportService(_ollamaClient, _pdfTextExtractor, options);
    }

    [Fact]
    public async Task ImportFromPdfAsync_CallsCoderModelWithExtractedText()
    {
        // Arrange
        _pdfTextExtractor.ExtractText(Arg.Any<byte[]>()).Returns(FakeExtractedText);
        _ollamaClient.GenerateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("<!DOCTYPE html><html><body><p>{{PolicyNumber}}</p></body></html>");

        // Act
        var result = await _sut.ImportFromPdfAsync(FakePdfBytes, CancellationToken.None);

        // Assert
        await _ollamaClient.Received(1).GenerateAsync(
            "qwen2.5-coder:7b",
            Arg.Is<string>(p => p.Contains(FakeExtractedText)),
            Arg.Any<CancellationToken>());

        result.Should().Contain("{{PolicyNumber}}");
    }

    [Fact]
    public async Task ImportFromPdfAsync_PassesPdfBytesToExtractor()
    {
        // Arrange
        _pdfTextExtractor.ExtractText(Arg.Any<byte[]>()).Returns(FakeExtractedText);
        _ollamaClient.GenerateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("<!DOCTYPE html><html><body></body></html>");

        // Act
        await _sut.ImportFromPdfAsync(FakePdfBytes, CancellationToken.None);

        // Assert
        _pdfTextExtractor.Received(1).ExtractText(FakePdfBytes);
    }

    [Theory]
    [InlineData(
        "Here is your template:\n```html\n<!DOCTYPE html><html><body>test</body></html>\n```",
        "<!DOCTYPE html><html><body>test</body></html>")]
    [InlineData(
        "<!DOCTYPE html><html><body>clean</body></html>",
        "<!DOCTYPE html><html><body>clean</body></html>")]
    [InlineData(
        "Some text before <!DOCTYPE html><html><body>x</body></html> text after",
        "<!DOCTYPE html><html><body>x</body></html>")]
    public void ExtractHtml_CleansMdFencesAndExtraText(string raw, string expected)
    {
        // Act
        var result = TemplateImportService.ExtractHtml(raw);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ExtractHtml_WhenNoHtmlFound_ReturnsTrimmedRaw()
    {
        // Arrange
        const string raw = "  Some plain text with no HTML.  ";

        // Act
        var result = TemplateImportService.ExtractHtml(raw);

        // Assert
        result.Should().Be("Some plain text with no HTML.");
    }
}
