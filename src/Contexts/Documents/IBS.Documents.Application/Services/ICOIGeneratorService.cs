namespace IBS.Documents.Application.Services;

/// <summary>
/// Service interface for generating Certificate of Insurance (COI) PDF documents.
/// </summary>
public interface ICOIGeneratorService
{
    /// <summary>
    /// Generates a COI PDF by rendering the given Handlebars template with the provided data.
    /// </summary>
    /// <param name="templateContent">The Handlebars HTML template content.</param>
    /// <param name="data">The data to bind into the template.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The generated PDF as a byte array.</returns>
    Task<byte[]> GenerateAsync(string templateContent, COITemplateData data, CancellationToken cancellationToken = default);
}
