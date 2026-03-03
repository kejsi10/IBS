namespace IBS.PolicyAssistant.Infrastructure.Ai;

/// <summary>
/// Configuration options for the Policy Assistant AI and search features.
/// Supports both local (Ollama + SQL FTS) and Azure (Azure OpenAI + Azure AI Search) providers.
/// </summary>
public sealed class PolicyAssistantOptions
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "PolicyAssistant";

    /// <summary>
    /// Gets or sets the provider to use. "Local" for Ollama+SQL FTS, "Azure" for Azure OpenAI+AI Search.
    /// Defaults to "Local".
    /// </summary>
    public string Provider { get; set; } = "Local";
}

/// <summary>
/// Configuration options for Azure OpenAI.
/// Bound from the "AzureOpenAI" configuration section.
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

/// <summary>
/// Configuration options for the Ollama local AI server.
/// Shared with the Documents context via the "Ollama" config section.
/// </summary>
public sealed class OllamaOptions
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "Ollama";

    /// <summary>
    /// Gets or sets the base URL of the Ollama server.
    /// </summary>
    public string BaseUrl { get; set; } = "http://localhost:11434";

    /// <summary>
    /// Gets or sets the model name to use for chat completions.
    /// </summary>
    public string ChatModel { get; set; } = "qwen2.5:3b";

    /// <summary>
    /// Gets or sets the HTTP request timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 300;
}
