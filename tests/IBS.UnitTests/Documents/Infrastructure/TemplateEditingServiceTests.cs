using FluentAssertions;
using IBS.Documents.Application.Services;
using IBS.Documents.Infrastructure.Ai;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace IBS.UnitTests.Documents.Infrastructure;

/// <summary>
/// Unit tests for TemplateEditingService.
/// IOllamaClient is mocked to isolate prompt construction and HTML extraction logic.
/// </summary>
public class TemplateEditingServiceTests
{
    private const string SampleTemplate =
        "<!DOCTYPE html><html><body><p>{{PolicyNumber}}</p></body></html>";

    private readonly IOllamaClient _ollamaClient = Substitute.For<IOllamaClient>();
    private readonly TemplateEditingService _sut;

    public TemplateEditingServiceTests()
    {
        var options = Options.Create(new OllamaOptions
        {
            CoderModel = "qwen2.5:3b"
        });

        _sut = new TemplateEditingService(_ollamaClient, options);
    }

    [Fact]
    public async Task EditTemplateAsync_CallsCoderModelWithCurrentContentAndInstruction()
    {
        // Arrange
        const string instruction = "Make the header font blue";
        _ollamaClient.GenerateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("<!DOCTYPE html><html><body><p style=\"color:blue\">{{PolicyNumber}}</p></body></html>");

        string capturedModel = string.Empty;
        string capturedPrompt = string.Empty;
        await _ollamaClient.GenerateAsync(
            Arg.Do<string>(m => capturedModel = m),
            Arg.Do<string>(p => capturedPrompt = p),
            Arg.Any<CancellationToken>());

        // Act
        await _sut.EditTemplateAsync(SampleTemplate, instruction, CancellationToken.None);

        // Assert
        capturedModel.Should().Be("qwen2.5:3b");
        capturedPrompt.Should().Contain(SampleTemplate);
        capturedPrompt.Should().Contain(instruction);
    }

    [Fact]
    public async Task EditTemplateAsync_ReturnsCleanHtmlFromModelResponse()
    {
        // Arrange
        const string modelResponse =
            "Sure! Here is the modified template:\n```html\n<!DOCTYPE html><html><body><p>{{PolicyNumber}}</p></body></html>\n```";
        _ollamaClient.GenerateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(modelResponse);

        // Act
        var result = await _sut.EditTemplateAsync(SampleTemplate, "any instruction", CancellationToken.None);

        // Assert
        result.Should().StartWith("<!DOCTYPE html>");
        result.Should().EndWith("</html>");
        result.Should().NotContain("```");
        result.Should().NotContain("Sure!");
    }

    [Fact]
    public async Task EditTemplateAsync_PreservesHandlebarsExpressionsInPrompt()
    {
        // Arrange
        const string instruction = "Change the background to grey";
        _ollamaClient.GenerateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("<!DOCTYPE html><html><body>{{PolicyNumber}}</body></html>");

        string capturedPrompt = string.Empty;
        await _ollamaClient.GenerateAsync(
            Arg.Any<string>(),
            Arg.Do<string>(p => capturedPrompt = p),
            Arg.Any<CancellationToken>());

        // Act
        await _sut.EditTemplateAsync(SampleTemplate, instruction, CancellationToken.None);

        // Assert — prompt must tell the model to preserve Handlebars
        capturedPrompt.Should().Contain("Handlebars");
        capturedPrompt.Should().Contain("{{PolicyNumber}}");
    }

    [Theory]
    [InlineData(
        "```html\n<!DOCTYPE html><html><body>x</body></html>\n```",
        "<!DOCTYPE html><html><body>x</body></html>")]
    [InlineData(
        "<!DOCTYPE html><html><body>clean</body></html>",
        "<!DOCTYPE html><html><body>clean</body></html>")]
    public void ExtractHtml_CleansMdFencesCorrectly(string raw, string expected)
    {
        // Act
        var result = TemplateEditingService.ExtractHtml(raw);

        // Assert
        result.Should().Be(expected);
    }
}
