using IBS.PolicyAssistant.Application.Services;
using IBS.PolicyAssistant.Domain.Queries;
using IBS.PolicyAssistant.Domain.Repositories;
using IBS.PolicyAssistant.Infrastructure.Ai;
using IBS.PolicyAssistant.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace IBS.PolicyAssistant.Infrastructure;

/// <summary>
/// Dependency injection extensions for the PolicyAssistant Infrastructure layer.
/// Selects Local (Ollama + SQL) or Azure (Azure OpenAI + AI Search) implementations
/// based on the "PolicyAssistant:Provider" configuration value.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds PolicyAssistant Infrastructure services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPolicyAssistantInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind options
        services.Configure<PolicyAssistantOptions>(configuration.GetSection(PolicyAssistantOptions.SectionName));
        services.Configure<OllamaOptions>(configuration.GetSection(OllamaOptions.SectionName));
        services.Configure<AzureOpenAIOptions>(configuration.GetSection(AzureOpenAIOptions.SectionName));

        // Register persistence (always SQL Server)
        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IReferenceDocumentRepository, ReferenceDocumentRepository>();
        services.AddScoped<IConversationQueries, ConversationQueries>();
        services.AddScoped<IReferenceDocumentQueries, ReferenceDocumentQueries>();

        // Config-driven AI + search provider selection
        var provider = configuration.GetValue<string>($"{PolicyAssistantOptions.SectionName}:Provider") ?? "Local";

        if (provider.Equals("Azure", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IChatCompletionService, AzureOpenAIChatService>();
            // SqlFullTextSearchService is the concrete type AzureAISearchService delegates to
            services.AddScoped<SqlFullTextSearchService>();
            services.AddScoped<IReferenceDocumentSearchService, AzureAISearchService>();
        }
        else
        {
            // Ollama HTTP client with chat model
            services.AddHttpClient<IChatCompletionService, OllamaChatService>((sp, client) =>
            {
                var opts = sp.GetRequiredService<IOptions<OllamaOptions>>().Value;
                client.BaseAddress = new Uri(opts.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(opts.TimeoutSeconds);
            });

            services.AddScoped<IReferenceDocumentSearchService, SqlFullTextSearchService>();
        }

        // AI pipeline services (provider-agnostic — use IChatCompletionService internally)
        services.AddScoped<IPolicyExtractionService, PolicyExtractionService>();
        services.AddScoped<IPolicyValidationService, PolicyValidationService>();

        return services;
    }
}
