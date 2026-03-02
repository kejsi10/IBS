using System.Text;
using System.Text.Json;
using IBS.PolicyAssistant.Application.DTOs;
using IBS.PolicyAssistant.Application.Services;

namespace IBS.PolicyAssistant.Infrastructure.Ai;

/// <summary>
/// Validates extracted policy data against insurance regulations using AI + RAG.
/// Provider-agnostic — uses <see cref="IChatCompletionService"/> and <see cref="IReferenceDocumentSearchService"/>.
/// </summary>
public sealed class PolicyValidationService(
    IChatCompletionService chatService,
    IReferenceDocumentSearchService searchService) : IPolicyValidationService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private const string ValidationSystemPromptTemplate = """
        You are an insurance compliance expert. Validate the policy data against the provided regulations.

        Return ONLY a JSON object with this exact structure (no other text):
        {
          "isValid": true,
          "issues": [],
          "warnings": [],
          "summary": "Policy data appears valid."
        }

        Each issue: { "field": "effectiveDate", "rule": "Min effective period", "description": "...", "severity": "Error" }
        Severity must be "Error" or "Warning".

        Use the provided regulatory context to validate. Check:
        - Required minimum coverages for the line of business and state
        - Premium ranges (flag if unusually low or high)
        - Effective period length (typically 6 or 12 months)
        - Required fields presence
        """;

    /// <inheritdoc />
    public async Task<PolicyValidationResult> ValidateAsync(PolicyExtractionResult extracted, CancellationToken ct)
    {
        // Search for relevant regulations
        var query = $"{extracted.LineOfBusiness} {extracted.PolicyType} insurance requirements";
        var docs = await searchService.SearchAsync(
            query,
            lineOfBusiness: extracted.LineOfBusiness,
            maxResults: 5,
            ct: ct);

        // Build context from retrieved documents
        var contextBuilder = new StringBuilder();
        foreach (var doc in docs)
        {
            contextBuilder.AppendLine($"[{doc.Category}] {doc.Title}:");
            contextBuilder.AppendLine(doc.Content);
            contextBuilder.AppendLine();
        }

        // Build the policy summary for validation
        var policyJson = JsonSerializer.Serialize(extracted, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        var messages = new List<ChatMessage>
        {
            new("system", ValidationSystemPromptTemplate),
            new("user",
                $"Regulatory Context:\n{contextBuilder}\n\nPolicy to Validate:\n{policyJson}\n\nValidate this policy against the regulations above.")
        };

        var rawResponse = await chatService.ChatAsync(messages, ct);

        return ParseValidationResult(rawResponse);
    }

    /// <summary>
    /// Parses the raw JSON validation response from the AI.
    /// Exposed as internal static for unit testing.
    /// </summary>
    internal static PolicyValidationResult ParseValidationResult(string rawJson)
    {
        var json = ExtractJson(rawJson);

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var issues = new List<ValidationIssue>();
            if (root.TryGetProperty("issues", out var issuesProp) && issuesProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var issue in issuesProp.EnumerateArray())
                {
                    issues.Add(new ValidationIssue(
                        GetString(issue, "field") ?? "Unknown",
                        GetString(issue, "rule") ?? "Unknown",
                        GetString(issue, "description") ?? string.Empty,
                        GetString(issue, "severity") ?? "Error"));
                }
            }

            var warnings = new List<string>();
            if (root.TryGetProperty("warnings", out var warningsProp) && warningsProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var w in warningsProp.EnumerateArray())
                {
                    var warning = w.GetString();
                    if (!string.IsNullOrWhiteSpace(warning))
                        warnings.Add(warning);
                }
            }

            var isValid = !root.TryGetProperty("isValid", out var validProp) || validProp.GetBoolean();
            var summary = GetString(root, "summary") ?? "Validation complete.";

            return new PolicyValidationResult(isValid, issues, warnings, summary);
        }
        catch
        {
            return new PolicyValidationResult(true, [], [], "Validation skipped due to parsing error.");
        }
    }

    private static string ExtractJson(string text)
    {
        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        if (start >= 0 && end > start)
            return text[start..(end + 1)];
        return "{}";
    }

    private static string? GetString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.String
            ? prop.GetString()
            : null;
    }
}
