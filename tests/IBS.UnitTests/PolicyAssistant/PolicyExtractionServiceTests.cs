using FluentAssertions;
using IBS.PolicyAssistant.Application.DTOs;
using IBS.PolicyAssistant.Infrastructure.Ai;

namespace IBS.UnitTests.PolicyAssistant;

/// <summary>
/// Unit tests for the <see cref="PolicyExtractionService"/> internal static parsing methods.
/// These tests cover JSON parsing logic without invoking the AI backend.
/// </summary>
public class PolicyExtractionServiceTests
{
    [Fact]
    public void ParseExtractionResult_ValidCompleteJson_ReturnsCorrectResult()
    {
        // Arrange
        const string json = """
            {
              "isComplete": true,
              "clientName": "Acme Corporation",
              "carrierName": "State Farm",
              "lineOfBusiness": "GeneralLiability",
              "policyType": "Commercial",
              "effectiveDate": "2026-01-01",
              "expirationDate": "2027-01-01",
              "billingType": "Agency",
              "paymentPlan": "Annual",
              "coverages": [
                { "code": "GL", "name": "General Liability", "premium": "5000", "limit": "1000000", "deductible": "0" }
              ],
              "missingFields": []
            }
            """;

        // Act
        var result = PolicyExtractionService.ParseExtractionResult(json);

        // Assert
        result.Should().NotBeNull();
        result.IsComplete.Should().BeTrue();
        result.ClientName.Should().Be("Acme Corporation");
        result.CarrierName.Should().Be("State Farm");
        result.LineOfBusiness.Should().Be("GeneralLiability");
        result.PolicyType.Should().Be("Commercial");
        result.EffectiveDate.Should().Be("2026-01-01");
        result.ExpirationDate.Should().Be("2027-01-01");
        result.BillingType.Should().Be("Agency");
        result.PaymentPlan.Should().Be("Annual");
        result.MissingFields.Should().BeEmpty();
    }

    [Fact]
    public void ParseExtractionResult_ValidJson_ParsesCoveragesCorrectly()
    {
        // Arrange
        const string json = """
            {
              "isComplete": false,
              "coverages": [
                { "code": "BI", "name": "Bodily Injury", "premium": "1200", "limit": "500000", "deductible": "250" },
                { "code": "PD", "name": "Property Damage", "premium": "800", "limit": "100000", "deductible": "500" }
              ],
              "missingFields": ["carrierName", "effectiveDate"]
            }
            """;

        // Act
        var result = PolicyExtractionService.ParseExtractionResult(json);

        // Assert
        result.Coverages.Should().HaveCount(2);

        result.Coverages[0].Code.Should().Be("BI");
        result.Coverages[0].Name.Should().Be("Bodily Injury");
        result.Coverages[0].Premium.Should().Be("1200");
        result.Coverages[0].Limit.Should().Be("500000");
        result.Coverages[0].Deductible.Should().Be("250");

        result.Coverages[1].Code.Should().Be("PD");
        result.Coverages[1].Name.Should().Be("Property Damage");
    }

    [Fact]
    public void ParseExtractionResult_ValidJson_ParsesMissingFieldsCorrectly()
    {
        // Arrange
        const string json = """
            {
              "isComplete": false,
              "missingFields": ["clientName", "effectiveDate", "carrierName"]
            }
            """;

        // Act
        var result = PolicyExtractionService.ParseExtractionResult(json);

        // Assert
        result.MissingFields.Should().BeEquivalentTo(["clientName", "effectiveDate", "carrierName"]);
    }

    [Fact]
    public void ParseExtractionResult_IsCompleteFalse_ReturnsIncompleteResult()
    {
        // Arrange
        const string json = """{ "isComplete": false, "clientName": "Partial Client", "coverages": [], "missingFields": [] }""";

        // Act
        var result = PolicyExtractionService.ParseExtractionResult(json);

        // Assert
        result.IsComplete.Should().BeFalse();
        result.ClientName.Should().Be("Partial Client");
    }

    [Fact]
    public void ParseExtractionResult_InvalidJson_ReturnsEmptyFallbackResult()
    {
        // Arrange
        const string invalidJson = "this is not valid json at all";

        // Act
        var result = PolicyExtractionService.ParseExtractionResult(invalidJson);

        // Assert
        result.Should().NotBeNull();
        result.IsComplete.Should().BeFalse();
        result.ClientName.Should().BeNull();
        result.CarrierName.Should().BeNull();
        result.Coverages.Should().BeEmpty();
        result.MissingFields.Should().BeEmpty();
    }

    [Fact]
    public void ParseExtractionResult_NullFieldValues_ReturnsNullForOptionalFields()
    {
        // Arrange
        const string json = """
            {
              "isComplete": false,
              "clientName": null,
              "carrierName": null,
              "lineOfBusiness": null,
              "coverages": [],
              "missingFields": []
            }
            """;

        // Act
        var result = PolicyExtractionService.ParseExtractionResult(json);

        // Assert
        result.ClientName.Should().BeNull();
        result.CarrierName.Should().BeNull();
        result.LineOfBusiness.Should().BeNull();
    }

    [Fact]
    public void ExtractJson_PlainJson_ReturnsSameJson()
    {
        // Arrange
        const string text = """{"isComplete":false,"clientName":"Acme"}""";

        // Act
        var result = PolicyExtractionService.ExtractJson(text);

        // Assert
        result.Should().Be(text);
    }

    [Fact]
    public void ExtractJson_JsonWrappedInMarkdown_ExtractsJsonOnly()
    {
        // Arrange
        const string text = """
            Here is the extracted data:
            ```json
            {"isComplete":true,"clientName":"Test Corp"}
            ```
            Let me know if you need anything else.
            """;

        // Act
        var result = PolicyExtractionService.ExtractJson(text);

        // Assert
        result.Should().Contain("isComplete");
        result.Should().StartWith("{");
        result.Should().EndWith("}");
    }

    [Fact]
    public void ExtractJson_JsonWithSurroundingText_ExtractsJsonPortion()
    {
        // Arrange
        const string text = """Sure! Here is the result: {"foo":"bar","baz":42} Hope that helps.""";

        // Act
        var result = PolicyExtractionService.ExtractJson(text);

        // Assert
        result.Should().Be("""{"foo":"bar","baz":42}""");
    }

    [Fact]
    public void ExtractJson_NoJsonPresent_ReturnsEmptyObject()
    {
        // Arrange
        const string text = "There is no JSON in this response at all.";

        // Act
        var result = PolicyExtractionService.ExtractJson(text);

        // Assert
        result.Should().Be("{}");
    }

    [Fact]
    public void ExtractJson_EmptyString_ReturnsEmptyObject()
    {
        // Arrange
        const string text = "";

        // Act
        var result = PolicyExtractionService.ExtractJson(text);

        // Assert
        result.Should().Be("{}");
    }
}
