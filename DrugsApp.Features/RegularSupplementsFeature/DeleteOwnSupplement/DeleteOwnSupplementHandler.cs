using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.RegularSupplementsFeature.ApplicationServices;
using DrugsApp.Features.RegularSupplementsFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.RegularSupplementsFeature.DeleteOwnSupplement;

/// <summary>
/// Handles deletion of own supplement compounds with cascade removal.
/// </summary>
internal sealed class DeleteOwnSupplementHandler(
    IClientOwnSupplementDeletionApplicationService _clientOwnSupplementDeletionApplicationService)
    : IRequestHandler<DeleteOwnSupplementCommand, ErrorOr<DeleteOwnSupplementResponse>>
{
    public async Task<ErrorOr<DeleteOwnSupplementResponse>> Handle(
        DeleteOwnSupplementCommand command,
        CancellationToken cancellationToken)
    {
        return await _clientOwnSupplementDeletionApplicationService.DeleteWithCascadeAsync(
            command.UserId,
            command.Id,
            cancellationToken);
    }
}
