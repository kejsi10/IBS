using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Claims.Domain.Repositories;

namespace IBS.Claims.Application.Commands.AddClaimNote;

/// <summary>
/// Handler for the AddClaimNoteCommand.
/// </summary>
public sealed class AddClaimNoteCommandHandler(
    IClaimRepository claimRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<AddClaimNoteCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(AddClaimNoteCommand request, CancellationToken cancellationToken)
    {
        var claim = await claimRepository.GetByIdAsync(request.ClaimId, cancellationToken);
        if (claim is null)
            return Error.NotFound("Claim", request.ClaimId);

        ConcurrencyGuard.Validate(request.ExpectedRowVersion, claim.RowVersion);

        claim.AddNote(request.Content, request.UserId.ToString(), request.IsInternal);

        await claimRepository.UpdateAsync(claim, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
