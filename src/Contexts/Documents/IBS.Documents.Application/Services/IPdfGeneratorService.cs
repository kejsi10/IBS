namespace IBS.Documents.Application.Services;

/// <summary>
/// Generic PDF generation service that renders a Handlebars HTML template with any data object.
/// </summary>
public interface IPdfGeneratorService
{
    /// <summary>
    /// Generates a PDF by rendering the Handlebars template with the provided data.
    /// </summary>
    /// <param name="templateContent">The Handlebars HTML template content.</param>
    /// <param name="data">The data object to bind into the template.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The generated PDF as a byte array.</returns>
    Task<byte[]> GenerateAsync(string templateContent, object data, CancellationToken cancellationToken = default);
}
