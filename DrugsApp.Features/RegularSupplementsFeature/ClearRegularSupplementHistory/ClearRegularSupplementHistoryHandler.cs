using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.RegularSupplementsFeature.Repositories;
using DrugsApp.Features.RegularSupplementsFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.RegularSupplementsFeature.ClearRegularSupplementHistory;

/// <summary>
/// Handles bulk deletion of archived regular supplement schedules.
/// </summary>
internal sealed class ClearRegularSupplementHistoryHandler(
    IClientRegularSupplementRepository _clientRegularSupplementRepository)
    : IRequestHandler<ClearRegularSupplementHistoryCommand, ErrorOr<ClearRegularSupplementHistoryResponse>>
{
    public async Task<ErrorOr<ClearRegularSupplementHistoryResponse>> Handle(
        ClearRegularSupplementHistoryCommand command,
        CancellationToken cancellationToken)
    {
        int deletedCount = await _clientRegularSupplementRepository.DeleteAllExpiredByUserIdAsync(
            command.UserId,
            cancellationToken);

        return new ClearRegularSupplementHistoryResponse(deletedCount);
    }
}
