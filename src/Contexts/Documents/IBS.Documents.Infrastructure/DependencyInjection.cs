using IBS.Documents.Application.Services;
using IBS.Documents.Domain.Queries;
using IBS.Documents.Domain.Repositories;
using IBS.Documents.Infrastructure.Ai;
using IBS.Documents.Infrastructure.Pdf;
using IBS.Documents.Infrastructure.Persistence;
using IBS.Documents.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace IBS.Documents.Infrastructure;

/// <summary>
/// Dependency injection extensions for the Documents Infrastructure layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Documents Infrastructure services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration (used for Ollama options).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDocumentsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Persistence
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IDocumentTemplateRepository, DocumentTemplateRepository>();
        services.AddScoped<IDocumentQueries, DocumentQueries>();
        services.AddScoped<IDocumentTemplateQueries, DocumentTemplateQueries>();

        // Storage
        services.AddScoped<IBlobStorageService, AzureBlobStorageService>();

        // Cross-context services
        services.AddScoped<IPolicyDataService, PolicyDataService>();

        // PDF generation — singleton browser, scoped service
        services.AddSingleton<IPlaywrightBrowserManager, PlaywrightBrowserManager>();
        services.AddScoped<ICOIGeneratorService, PlaywrightPdfGeneratorService>();

        // AI services — config-driven provider selection
        services.Configure<OllamaOptions>(configuration.GetSection(OllamaOptions.SectionName));
        services.Configure<AzureOpenAIOptions>(configuration.GetSection(AzureOpenAIOptions.SectionName));

        services.AddSingleton<IPdfTextExtractor, PdfPigTextExtractor>();

        var provider = configuration.GetValue<string>("Documents:Provider") ?? "Local";

        if (provider.Equals("Azure", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<ITemplateEditingService, AzureOpenAITemplateEditingService>();
            services.AddScoped<ITemplateImportService, AzureOpenAITemplateImportService>();
        }
        else
        {
            services.AddHttpClient<IOllamaClient, OllamaClient>((sp, client) =>
            {
                var opts = sp.GetRequiredService<IOptions<OllamaOptions>>().Value;
                client.BaseAddress = new Uri(opts.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(opts.TimeoutSeconds);
            });

            services.AddScoped<ITemplateImportService, TemplateImportService>();
            services.AddScoped<ITemplateEditingService, TemplateEditingService>();
        }

        return services;
    }
}
