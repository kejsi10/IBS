using IBS.Documents.Application.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace IBS.Documents.Infrastructure.Persistence;

/// <summary>
/// Cross-context implementation of IQuoteDataService that fetches quote data
/// needed for proposal generation using the shared DbContext.
/// </summary>
public sealed class QuoteDataService : IQuoteDataService
{
    private readonly DbContext _context;

    /// <summary>
    /// Initializes a new instance of the QuoteDataService class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public QuoteDataService(DbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<QuoteProposalData?> GetQuoteProposalDataAsync(Guid quoteId, CancellationToken cancellationToken = default)
    {
        var headerSql = """
            SELECT
                COALESCE(
                    cl.BusinessName,
                    NULLIF(LTRIM(RTRIM(ISNULL(cl.FirstName, '') + ' ' + ISNULL(cl.LastName, ''))), '')
                ) AS ClientName,
                NULLIF(LTRIM(RTRIM(
                    ISNULL(cl.Address1, '') +
                    CASE WHEN cl.City IS NOT NULL THEN ', ' + cl.City ELSE '' END +
                    CASE WHEN cl.Province IS NOT NULL THEN ', ' + cl.Province ELSE '' END +
                    CASE WHEN cl.PostalCode IS NOT NULL THEN ' ' + cl.PostalCode ELSE '' END
                )), '') AS ClientAddress,
                q.LineOfBusiness,
                q.EffectiveDate,
                q.ExpirationDate,
                q.Notes
            FROM Quotes q
            INNER JOIN Clients cl ON cl.Id = q.ClientId
            WHERE q.Id = {0}
            """;

        var headers = await _context.Database
            .SqlQueryRaw<QuoteHeaderRow>(headerSql, new SqlParameter("@p0", quoteId))
            .ToListAsync(cancellationToken);

        var header = headers.FirstOrDefault();
        if (header is null) return null;

        var carrierSql = """
            SELECT
                ca.Name AS CarrierName,
                qc.Status,
                qc.PremiumAmount,
                qc.Conditions,
                qc.ProposedCoverages
            FROM QuoteCarriers qc
            INNER JOIN Carriers ca ON ca.Id = qc.CarrierId
            WHERE qc.QuoteId = {0}
            ORDER BY qc.CreatedAt
            """;

        var carriers = await _context.Database
            .SqlQueryRaw<QuoteCarrierRow>(carrierSql, new SqlParameter("@p0", quoteId))
            .ToListAsync(cancellationToken);

        return new QuoteProposalData
        {
            ClientName = header.ClientName,
            ClientAddress = header.ClientAddress,
            LineOfBusiness = header.LineOfBusiness,
            EffectiveDate = DateOnly.FromDateTime(header.EffectiveDate),
            ExpirationDate = DateOnly.FromDateTime(header.ExpirationDate),
            Notes = header.Notes,
            CarrierOffers = carriers.Select(c => new QuoteCarrierProposalData
            {
                CarrierName = c.CarrierName,
                Status = c.Status,
                PremiumAmount = c.PremiumAmount,
                Conditions = c.Conditions,
                ProposedCoverages = c.ProposedCoverages
            }).ToList()
        };
    }

    // Private projection classes for raw SQL queries.

    /// <summary>Quote header projection row.</summary>
    private sealed class QuoteHeaderRow
    {
        public string ClientName { get; set; } = string.Empty;
        public string? ClientAddress { get; set; }
        public string LineOfBusiness { get; set; } = string.Empty;
        public DateTime EffectiveDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>Quote carrier projection row.</summary>
    private sealed class QuoteCarrierRow
    {
        public string CarrierName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal? PremiumAmount { get; set; }
        public string? Conditions { get; set; }
        public string? ProposedCoverages { get; set; }
    }
}
