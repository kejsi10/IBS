using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Policies.Domain.Repositories;

namespace IBS.Policies.Application.Commands.ExpireQuotes;

/// <summary>
/// Handler for the ExpireQuotesCommand.
/// </summary>
public sealed class ExpireQuotesCommandHandler(
    IQuoteRepository quoteRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<ExpireQuotesCommand, int>
{
    /// <inheritdoc />
    public async Task<Result<int>> Handle(ExpireQuotesCommand request, CancellationToken cancellationToken)
    {
        var asOfDate = request.AsOfDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var expiredQuotes = await quoteRepository.GetExpiredQuotesAsync(asOfDate, cancellationToken);

        foreach (var quote in expiredQuotes)
        {
            quote.Expire();
        }

        if (expiredQuotes.Count > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return expiredQuotes.Count;
    }
}
