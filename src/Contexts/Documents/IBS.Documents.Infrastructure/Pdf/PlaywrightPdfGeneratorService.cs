using HandlebarsDotNet;
using IBS.Documents.Application.Services;
using Microsoft.Playwright;

namespace IBS.Documents.Infrastructure.Pdf;

/// <summary>
/// Playwright-based implementation of IPdfGeneratorService.
/// Renders a Handlebars HTML template with any data object, then converts it to a PDF
/// using a headless Chromium browser via Playwright.
/// </summary>
public sealed class PlaywrightPdfGeneratorService(IPlaywrightBrowserManager browserManager) : IPdfGeneratorService
{
    /// <inheritdoc />
    public async Task<byte[]> GenerateAsync(
        string templateContent,
        object data,
        CancellationToken cancellationToken = default)
    {
        // Compile and render the Handlebars template with the provided data
        var template = Handlebars.Compile(templateContent);
        var html = template(data);

        // Use the shared browser; each request gets its own page
        var browser = await browserManager.GetBrowserAsync();
        var page = await browser.NewPageAsync();
        try
        {
            await page.SetContentAsync(html, new PageSetContentOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded
            });

            return await page.PdfAsync(new PagePdfOptions
            {
                Format = "Letter",
                PrintBackground = true
            });
        }
        finally
        {
            await page.CloseAsync();
        }
    }
}
