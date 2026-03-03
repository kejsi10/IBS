namespace IBS.Documents.Infrastructure.Ai;

/// <summary>
/// Configuration options for the Ollama AI server connection.
/// Bound from the "Ollama" configuration section.
/// </summary>
public sealed class OllamaOptions
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = "Ollama";

    /// <summary>Gets or sets the base URL of the Ollama server (e.g., "http://localhost:11434").</summary>
    public string BaseUrl { get; set; } = "http://localhost:11434";

    /// <summary>Gets or sets the coder model identifier used for template editing and PDF import (e.g., "qwen2.5:3b").</summary>
    public string CoderModel { get; set; } = "qwen2.5:3b";

    /// <summary>Gets or sets the HTTP request timeout in seconds. Defaults to 300 for large model inference.</summary>
    public int TimeoutSeconds { get; set; } = 300;
}
