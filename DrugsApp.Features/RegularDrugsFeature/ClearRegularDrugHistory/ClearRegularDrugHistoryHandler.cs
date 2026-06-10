using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.RegularDrugsFeature.Interfaces;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.RegularDrugsFeature.ClearRegularDrugHistory;

/// <summary>
/// Handles bulk deletion of archived regular drug schedules.
/// </summary>
internal sealed class ClearRegularDrugHistoryHandler(
    IClientRegularDrugRepository _clientRegularDrugRepository)
    : IRequestHandler<ClearRegularDrugHistoryCommand, ErrorOr<ClearRegularDrugHistoryResponse>>
{
    public async Task<ErrorOr<ClearRegularDrugHistoryResponse>> Handle(
        ClearRegularDrugHistoryCommand command,
        CancellationToken cancellationToken)
    {
        int deletedCount = await _clientRegularDrugRepository.DeleteExpiredByUserIdAsync(
            command.UserId,
            cancellationToken);

        return new ClearRegularDrugHistoryResponse(deletedCount);
    }
}
