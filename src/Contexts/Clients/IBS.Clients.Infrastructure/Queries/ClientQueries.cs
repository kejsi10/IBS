using IBS.Clients.Application.Queries;
using IBS.Clients.Domain.Aggregates.Client;
using IBS.Clients.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace IBS.Clients.Infrastructure.Queries;

/// <summary>
/// Read-only query implementation for Client data.
/// All queries use AsNoTracking for optimal performance.
/// </summary>
public sealed class ClientQueries : IClientQueries
{
    private readonly DbContext _context;
    private readonly DbSet<Client> _clients;

    /// <summary>
    /// Initializes a new instance of the ClientQueries class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public ClientQueries(DbContext context)
    {
        _context = context;
        _clients = context.Set<Client>();
    }

    /// <inheritdoc />
    public async Task<ClientDetailsDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var client = await _clients
            .AsNoTracking()
            .Include(c => c.Contacts)
            .Include(c => c.Addresses)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (client is null)
            return null;

        return new ClientDetailsDto
        {
            Id = client.Id,
            ClientType = client.ClientType.ToString(),
            Status = client.Status.ToString(),
            DisplayName = client.GetDisplayName(),
            PersonName = client.PersonName is not null
                ? new PersonNameDto(
                    client.PersonName.FirstName,
                    client.PersonName.MiddleName,
                    client.PersonName.LastName,
                    client.PersonName.Suffix)
                : null,
            DateOfBirth = client.DateOfBirth,
            BusinessInfo = client.BusinessInfo is not null
                ? new BusinessInfoDto(
                    client.BusinessInfo.Name,
                    client.BusinessInfo.DbaName,
                    client.BusinessInfo.BusinessType,
                    client.BusinessInfo.Industry,
                    client.BusinessInfo.YearEstablished,
                    client.BusinessInfo.NumberOfEmployees,
                    client.BusinessInfo.AnnualRevenue,
                    client.BusinessInfo.Website)
                : null,
            Email = client.Email?.Value,
            Phone = client.Phone?.Format(),
            Contacts = client.Contacts.Select(c => new ContactDto(
                c.Id,
                c.Name.FirstName,
                c.Name.LastName,
                c.Title,
                c.Email?.Value,
                c.Phone?.Format(),
                c.IsPrimary)).ToList(),
            Addresses = client.Addresses.Select(a => new AddressDto(
                a.Id,
                a.AddressType.ToString(),
                a.StreetLine1,
                a.StreetLine2,
                a.City,
                a.State,
                a.PostalCode,
                a.Country,
                a.IsPrimary)).ToList(),
            CreatedAt = client.CreatedAt,
            UpdatedAt = client.UpdatedAt
        };
    }

    /// <inheritdoc />
    public async Task<PagedResult<ClientListItemDto>> SearchAsync(
        string? searchTerm,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _clients.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(c =>
                (c.PersonName != null && (
                    c.PersonName.FirstName.ToLower().Contains(term) ||
                    c.PersonName.LastName.ToLower().Contains(term))) ||
                (c.BusinessInfo != null && c.BusinessInfo.Name.ToLower().Contains(term)) ||
                (c.Email != null && c.Email.Value.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new ClientListItemDto
            {
                Id = c.Id,
                ClientType = c.ClientType.ToString(),
                DisplayName = c.ClientType == ClientType.Individual
                    ? c.PersonName!.FirstName + " " + c.PersonName!.LastName
                    : c.BusinessInfo!.Name,
                Email = c.Email != null ? c.Email.Value : null,
                Phone = c.Phone != null ? c.Phone.Value : null,
                Status = c.Status.ToString(),
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ClientListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ClientListItemDto>> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLower();

        return await _clients
            .AsNoTracking()
            .Where(c => c.Email != null && c.Email.Value.ToLower() == normalizedEmail)
            .Select(c => new ClientListItemDto
            {
                Id = c.Id,
                ClientType = c.ClientType.ToString(),
                DisplayName = c.ClientType == ClientType.Individual
                    ? c.PersonName!.FirstName + " " + c.PersonName!.LastName
                    : c.BusinessInfo!.Name,
                Email = c.Email != null ? c.Email.Value : null,
                Phone = c.Phone != null ? c.Phone.Value : null,
                Status = c.Status.ToString(),
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> EmailExistsAsync(
        string email,
        Guid? excludeClientId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLower();

        var query = _clients
            .AsNoTracking()
            .Where(c => c.Email != null && c.Email.Value.ToLower() == normalizedEmail);

        if (excludeClientId.HasValue)
        {
            query = query.Where(c => c.Id != excludeClientId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}
