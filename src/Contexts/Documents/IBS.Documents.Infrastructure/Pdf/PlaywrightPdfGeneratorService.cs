using HandlebarsDotNet;
using IBS.Documents.Application.Services;
using Microsoft.Playwright;

namespace IBS.Documents.Infrastructure.Pdf;

/// <summary>
/// Playwright-based implementation of ICOIGeneratorService.
/// Renders the Handlebars HTML template with policy data, then converts it to a PDF
/// using a headless Chromium browser via Playwright.
/// </summary>
public sealed class PlaywrightPdfGeneratorService(IPlaywrightBrowserManager browserManager) : ICOIGeneratorService
{
    /// <inheritdoc />
    public async Task<byte[]> GenerateAsync(
        string templateContent,
        COITemplateData data,
        CancellationToken cancellationToken = default)
    {
        // Compile and render the Handlebars template with policy data
        var template = Handlebars.Compile(templateContent);
        var html = template(new
        {
            data.PolicyNumber,
            data.ClientName,
            data.CarrierName,
            EffectiveDate = data.EffectiveDate.ToString("MM/dd/yyyy"),
            ExpirationDate = data.ExpirationDate.ToString("MM/dd/yyyy"),
            data.LineOfBusiness,
            data.CoverageSummary,
            data.BrokerName,
            IssuedDate = data.IssuedDate.ToString("MM/dd/yyyy")
        });

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
