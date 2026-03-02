using Azure;
using Azure.AI.OpenAI;
using IBS.PolicyAssistant.Application.Services;
using Microsoft.Extensions.Options;
using AppChatMessage = IBS.PolicyAssistant.Application.Services.ChatMessage;

namespace IBS.PolicyAssistant.Infrastructure.Ai;

/// <summary>
/// Azure OpenAI implementation of <see cref="IChatCompletionService"/>.
/// Reads configuration from <see cref="AzureOpenAIOptions"/>.
/// </summary>
public sealed class AzureOpenAIChatService(IOptions<AzureOpenAIOptions> options) : IChatCompletionService
{
    private readonly AzureOpenAIOptions _options = options.Value;

    /// <inheritdoc />
    public async Task<string> ChatAsync(IReadOnlyList<AppChatMessage> messages, CancellationToken ct)
    {
        var client = new AzureOpenAIClient(
            new Uri(_options.Endpoint),
            new AzureKeyCredential(_options.ApiKey));

        var chatClient = client.GetChatClient(_options.DeploymentName);

        var sdkMessages = messages.Select<AppChatMessage, OpenAI.Chat.ChatMessage>(m => m.Role switch
        {
            "system" => OpenAI.Chat.ChatMessage.CreateSystemMessage(m.Content),
            "assistant" => OpenAI.Chat.ChatMessage.CreateAssistantMessage(m.Content),
            _ => OpenAI.Chat.ChatMessage.CreateUserMessage(m.Content)
        }).ToList();

        var response = await chatClient.CompleteChatAsync(sdkMessages, cancellationToken: ct);
        return response.Value.Content[0].Text;
    }
}
