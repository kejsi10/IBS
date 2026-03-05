using Microsoft.Playwright;

namespace IBS.Documents.Infrastructure.Pdf;

/// <summary>
/// Singleton manager for a shared Playwright Chromium browser instance.
/// Lazily launches Chromium on first request and reuses it across PDF generation calls.
/// Each call to GenerateAsync creates and disposes its own page.
/// </summary>
public sealed class PlaywrightBrowserManager : IPlaywrightBrowserManager
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    /// <inheritdoc />
    public async Task<IBrowser> GetBrowserAsync()
    {
        if (_browser is not null)
            return _browser;

        await _lock.WaitAsync();
        try
        {
            if (_browser is not null)
                return _browser;

            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
                // Required when running as non-root inside a container
                Args = ["--no-sandbox", "--disable-setuid-sandbox"]
            });
            return _browser;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_browser is not null)
            await _browser.DisposeAsync();
        _playwright?.Dispose();
    }
}
