using System.Text;
using System.Text.Json;
using IBS.PolicyAssistant.Application.DTOs;
using IBS.PolicyAssistant.Application.Services;

namespace IBS.PolicyAssistant.Infrastructure.Ai;

/// <summary>
/// Extracts structured policy data from conversation history using AI.
/// Provider-agnostic — uses <see cref="IChatCompletionService"/> which can be Ollama or Azure OpenAI.
/// </summary>
public sealed class PolicyExtractionService(IChatCompletionService chatService) : IPolicyExtractionService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private const string ExtractionSystemPrompt = """
        You are an insurance policy data extraction assistant. Analyze the conversation and extract structured policy information.

        Return ONLY a JSON object with this exact structure (no other text):
        {
          "isComplete": false,
          "clientName": null,
          "carrierName": null,
          "lineOfBusiness": null,
          "policyType": null,
          "effectiveDate": null,
          "expirationDate": null,
          "billingType": null,
          "paymentPlan": null,
          "coverages": [],
          "missingFields": []
        }

        Rules:
        - lineOfBusiness must be one of: PersonalAuto, Homeowners, Renters, PersonalUmbrella, GeneralLiability, CommercialProperty, WorkersCompensation, CommercialAuto, ProfessionalLiability, CyberLiability, BusinessOwnersPolicy
        - dates must be in ISO 8601 format (YYYY-MM-DD)
        - billingType: "Agency" or "DirectBill"
        - paymentPlan: "Annual", "SemiAnnual", "Quarterly", or "Monthly"
        - Set isComplete to true only if all required fields are present and valid
        - List missing required fields in missingFields array
        - Each coverage: { "code": "GL", "name": "General Liability", "premium": "5000", "limit": "1000000", "deductible": "0" }
        """;

    /// <inheritdoc />
    public async Task<PolicyExtractionResult> ExtractAsync(IReadOnlyList<ChatMessage> messages, CancellationToken ct)
    {
        // Build a summary prompt with conversation to extract from
        var conversationText = new StringBuilder();
        foreach (var msg in messages.Where(m => m.Role is "user" or "assistant"))
        {
            conversationText.AppendLine($"{msg.Role.ToUpperInvariant()}: {msg.Content}");
        }

        var extractionMessages = new List<ChatMessage>
        {
            new("system", ExtractionSystemPrompt),
            new("user", $"Extract policy data from this conversation:\n\n{conversationText}")
        };

        var rawResponse = await chatService.ChatAsync(extractionMessages, ct);

        return ParseExtractionResult(rawResponse);
    }

    /// <summary>
    /// Parses the raw JSON string from the AI into a <see cref="PolicyExtractionResult"/>.
    /// Exposed as internal static for unit testing.
    /// </summary>
    /// <param name="rawJson">The raw JSON string from the AI.</param>
    /// <returns>The parsed result, or an empty result if parsing fails.</returns>
    internal static PolicyExtractionResult ParseExtractionResult(string rawJson)
    {
        // Extract JSON object from the response (AI may wrap it in markdown)
        var json = ExtractJson(rawJson);

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var coverages = new List<ExtractedCoverage>();
            if (root.TryGetProperty("coverages", out var coveragesProp) && coveragesProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var c in coveragesProp.EnumerateArray())
                {
                    coverages.Add(new ExtractedCoverage(
                        GetString(c, "code"),
                        GetString(c, "name"),
                        GetString(c, "premium"),
                        GetString(c, "limit"),
                        GetString(c, "deductible")));
                }
            }

            var missingFields = new List<string>();
            if (root.TryGetProperty("missingFields", out var missingProp) && missingProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var f in missingProp.EnumerateArray())
                {
                    var field = f.GetString();
                    if (!string.IsNullOrWhiteSpace(field))
                        missingFields.Add(field);
                }
            }

            var isComplete = root.TryGetProperty("isComplete", out var completeProp)
                && completeProp.GetBoolean();

            return new PolicyExtractionResult(
                isComplete,
                GetString(root, "clientName"),
                GetString(root, "carrierName"),
                GetString(root, "lineOfBusiness"),
                GetString(root, "policyType"),
                GetString(root, "effectiveDate"),
                GetString(root, "expirationDate"),
                GetString(root, "billingType"),
                GetString(root, "paymentPlan"),
                coverages,
                missingFields);
        }
        catch
        {
            return new PolicyExtractionResult(false, null, null, null, null, null, null, null, null, [], []);
        }
    }

    /// <summary>
    /// Extracts a JSON object from a string that may contain markdown or other text.
    /// </summary>
    internal static string ExtractJson(string text)
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
