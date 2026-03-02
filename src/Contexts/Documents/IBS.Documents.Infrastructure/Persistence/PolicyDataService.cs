using IBS.Documents.Application.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace IBS.Documents.Infrastructure.Persistence;

/// <summary>
/// Cross-context implementation of IPolicyDataService that fetches policy data
/// needed for COI generation using the shared DbContext.
/// </summary>
public sealed class PolicyDataService : IPolicyDataService
{
    private readonly DbContext _context;

    /// <summary>
    /// Initializes a new instance of the PolicyDataService class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public PolicyDataService(DbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<PolicyCOIData?> GetPolicyCOIDataAsync(Guid policyId, CancellationToken cancellationToken = default)
    {
        var sql = """
            SELECT
                p.PolicyNumber,
                COALESCE(
                    cl.BusinessName,
                    NULLIF(LTRIM(RTRIM(ISNULL(cl.FirstName, '') + ' ' + ISNULL(cl.LastName, ''))), '')
                ) AS ClientName,
                ca.Name AS CarrierName,
                p.EffectiveDate,
                p.ExpirationDate,
                p.LineOfBusiness
            FROM Policies p
            INNER JOIN Clients cl ON cl.Id = p.ClientId
            INNER JOIN Carriers ca ON ca.Id = p.CarrierId
            WHERE p.Id = {0}
            """;

        // Use an explicit named SqlParameter to make the parameterisation unambiguous
        var rows = await _context.Database
            .SqlQueryRaw<PolicyCOIRow>(sql, new SqlParameter("@p0", policyId))
            .ToListAsync(cancellationToken);

        var row = rows.FirstOrDefault();
        if (row is null) return null;

        return new PolicyCOIData(
            row.PolicyNumber,
            row.ClientName,
            row.CarrierName,
            new DateTimeOffset(row.EffectiveDate, TimeSpan.Zero),
            new DateTimeOffset(row.ExpirationDate, TimeSpan.Zero),
            row.LineOfBusiness,
            [row.LineOfBusiness]);
    }

    // Private projection class for raw SQL query.
    // EffectiveDate/ExpirationDate are SQL 'date' columns — ADO.NET returns them as DateTime.
    private sealed class PolicyCOIRow
    {
        public string PolicyNumber { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string CarrierName { get; set; } = string.Empty;
        public DateTime EffectiveDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string LineOfBusiness { get; set; } = string.Empty;
    }
}
