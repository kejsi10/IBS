namespace IBS.Documents.Infrastructure.Ai;

/// <summary>
/// Configuration options for Azure OpenAI used by the Documents AI features
/// (template editing and PDF import). Bound from the "AzureOpenAI" configuration section.
/// </summary>
public sealed class AzureOpenAIOptions
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "AzureOpenAI";

    /// <summary>
    /// Gets or sets the Azure OpenAI endpoint URL.
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the deployment name (model alias) in Azure OpenAI.
    /// </summary>
    public string DeploymentName { get; set; } = "gpt-4o-mini";

    /// <summary>
    /// Gets or sets the Azure OpenAI API key.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
}
