using System.ClientModel;
using IBS.PolicyAssistant.Application.Services;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using AppChatMessage = IBS.PolicyAssistant.Application.Services.ChatMessage;

namespace IBS.PolicyAssistant.Infrastructure.Ai;

/// <summary>
/// OpenAI-compatible implementation of <see cref="IChatCompletionService"/>.
/// Works with the standard OpenAI API (api.openai.com) or any OpenAI-compatible endpoint.
/// Reads configuration from <see cref="AzureOpenAIOptions"/>.
/// </summary>
public sealed class AzureOpenAIChatService(IOptions<AzureOpenAIOptions> options) : IChatCompletionService
{
    private readonly AzureOpenAIOptions _options = options.Value;

    /// <inheritdoc />
    public async Task<string> ChatAsync(IReadOnlyList<AppChatMessage> messages, CancellationToken ct)
    {
        var clientOptions = new OpenAIClientOptions { Endpoint = new Uri(_options.Endpoint) };
        var client = new OpenAIClient(new ApiKeyCredential(_options.ApiKey), clientOptions);
        var chatClient = client.GetChatClient(_options.DeploymentName);

        var sdkMessages = messages.Select<AppChatMessage, ChatMessage>(m => m.Role switch
        {
            "system" => ChatMessage.CreateSystemMessage(m.Content),
            "assistant" => ChatMessage.CreateAssistantMessage(m.Content),
            _ => ChatMessage.CreateUserMessage(m.Content)
        }).ToList();

        var response = await chatClient.CompleteChatAsync(sdkMessages, cancellationToken: ct);
        return response.Value.Content[0].Text;
    }
}
