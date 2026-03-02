using FluentAssertions;
using IBS.PolicyAssistant.Infrastructure.Ai;

namespace IBS.UnitTests.PolicyAssistant;

/// <summary>
/// Unit tests for the <see cref="PolicyValidationService"/> internal static parsing method.
/// Tests cover JSON parsing of the AI validation response without invoking AI or search services.
/// </summary>
public class PolicyValidationServiceTests
{
    [Fact]
    public void ParseValidationResult_ValidJsonWithNoIssues_ReturnsValidResult()
    {
        // Arrange
        const string json = """
            {
              "isValid": true,
              "issues": [],
              "warnings": [],
              "summary": "Policy data appears valid."
            }
            """;

        // Act
        var result = PolicyValidationService.ParseValidationResult(json);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Issues.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
        result.Summary.Should().Be("Policy data appears valid.");
    }

    [Fact]
    public void ParseValidationResult_ValidJsonWithIssues_ParsesIssuesCorrectly()
    {
        // Arrange
        const string json = """
            {
              "isValid": false,
              "issues": [
                {
                  "field": "effectiveDate",
                  "rule": "Minimum effective period",
                  "description": "Effective date cannot be in the past.",
                  "severity": "Error"
                },
                {
                  "field": "coverages",
                  "rule": "Minimum GL coverage",
                  "description": "GL policy requires at least $1,000,000 occurrence limit.",
                  "severity": "Error"
                }
              ],
              "warnings": [],
              "summary": "Policy has 2 blocking issues."
            }
            """;

        // Act
        var result = PolicyValidationService.ParseValidationResult(json);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Issues.Should().HaveCount(2);

        result.Issues[0].Field.Should().Be("effectiveDate");
        result.Issues[0].Rule.Should().Be("Minimum effective period");
        result.Issues[0].Description.Should().Be("Effective date cannot be in the past.");
        result.Issues[0].Severity.Should().Be("Error");

        result.Issues[1].Field.Should().Be("coverages");
        result.Issues[1].Rule.Should().Be("Minimum GL coverage");
        result.Issues[1].Severity.Should().Be("Error");
    }

    [Fact]
    public void ParseValidationResult_ValidJsonWithWarnings_ParsesWarningsCorrectly()
    {
        // Arrange
        const string json = """
            {
              "isValid": true,
              "issues": [],
              "warnings": ["Premium appears unusually low for the coverage amount.", "Payment plan not specified."],
              "summary": "Policy is valid with warnings."
            }
            """;

        // Act
        var result = PolicyValidationService.ParseValidationResult(json);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Warnings.Should().HaveCount(2);
        result.Warnings[0].Should().Be("Premium appears unusually low for the coverage amount.");
        result.Warnings[1].Should().Be("Payment plan not specified.");
    }

    [Fact]
    public void ParseValidationResult_IsValidFalse_ReturnsInvalidResult()
    {
        // Arrange
        const string json = """
            {
              "isValid": false,
              "issues": [{ "field": "clientName", "rule": "Required", "description": "Client name is missing.", "severity": "Error" }],
              "warnings": [],
              "summary": "Missing required fields."
            }
            """;

        // Act
        var result = PolicyValidationService.ParseValidationResult(json);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Issues.Should().HaveCount(1);
        result.Summary.Should().Be("Missing required fields.");
    }

    [Fact]
    public void ParseValidationResult_WarningSeverityIssue_ParsesSeverityCorrectly()
    {
        // Arrange
        const string json = """
            {
              "isValid": true,
              "issues": [
                { "field": "premium", "rule": "Premium range", "description": "Premium is low.", "severity": "Warning" }
              ],
              "warnings": [],
              "summary": "Minor warnings only."
            }
            """;

        // Act
        var result = PolicyValidationService.ParseValidationResult(json);

        // Assert
        result.Issues[0].Severity.Should().Be("Warning");
    }

    [Fact]
    public void ParseValidationResult_InvalidJson_ReturnsFallbackValidResult()
    {
        // Arrange — AI returned garbage
        const string invalidJson = "I'm sorry, I cannot validate this policy right now.";

        // Act
        var result = PolicyValidationService.ParseValidationResult(invalidJson);

        // Assert — fallback must be a "pass-through" valid result so the user is not blocked
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Issues.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
        result.Summary.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void ParseValidationResult_EmptyJson_ReturnsFallbackValidResult()
    {
        // Arrange
        const string empty = "";

        // Act
        var result = PolicyValidationService.ParseValidationResult(empty);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Issues.Should().BeEmpty();
    }

    [Fact]
    public void ParseValidationResult_JsonMissingIsValidField_DefaultsToValid()
    {
        // Arrange — response omits "isValid" entirely
        const string json = """
            {
              "issues": [],
              "warnings": [],
              "summary": "No issues found."
            }
            """;

        // Act
        var result = PolicyValidationService.ParseValidationResult(json);

        // Assert — per implementation: missing isValid defaults to true
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ParseValidationResult_JsonWrappedInMarkdown_ParsesCorrectly()
    {
        // Arrange — AI wraps response in a markdown code block
        const string json = """
            Sure, here is the validation result:
            ```json
            {"isValid":true,"issues":[],"warnings":[],"summary":"All good."}
            ```
            """;

        // Act
        var result = PolicyValidationService.ParseValidationResult(json);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Summary.Should().Be("All good.");
    }
}
