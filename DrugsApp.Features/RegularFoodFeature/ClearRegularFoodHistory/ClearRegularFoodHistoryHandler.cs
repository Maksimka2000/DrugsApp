using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.RegularFoodFeature.Interfaces;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.RegularFoodFeature.ClearRegularFoodHistory;

/// <summary>
/// Handles bulk deletion of expired regular food history rows.
/// </summary>
internal sealed class ClearRegularFoodHistoryHandler(
    IClientRegularFoodRepository _clientRegularFoodRepository)
    : IRequestHandler<ClearRegularFoodHistoryCommand, ErrorOr<ClearRegularFoodHistoryResponse>>
{
    public async Task<ErrorOr<ClearRegularFoodHistoryResponse>> Handle(
        ClearRegularFoodHistoryCommand command,
        CancellationToken cancellationToken)
    {
        int deletedCount = await _clientRegularFoodRepository.RemoveExpiredByUserIdAsync(
            command.UserId,
            cancellationToken);

        return new ClearRegularFoodHistoryResponse(deletedCount);
    }
}
