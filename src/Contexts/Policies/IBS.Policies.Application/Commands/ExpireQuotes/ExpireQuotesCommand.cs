using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Policies.Application.Commands.ExpireQuotes;

/// <summary>
/// Command to bulk expire past-due quotes.
/// </summary>
public sealed record ExpireQuotesCommand(
    DateOnly? AsOfDate = null
) : ICommand<int>;
