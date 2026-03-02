using Microsoft.Playwright;

namespace IBS.Documents.Infrastructure.Pdf;

/// <summary>
/// Manages the lifecycle of a singleton Playwright browser instance for PDF generation.
/// </summary>
public interface IPlaywrightBrowserManager : IAsyncDisposable
{
    /// <summary>
    /// Returns the shared browser instance, launching Chromium on first call.
    /// </summary>
    Task<IBrowser> GetBrowserAsync();
}
