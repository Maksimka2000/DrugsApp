using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.RegularDrugsFeature.ApplicationServices;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.RegularDrugsFeature.UpdateRegularDrug;

/// <summary>
/// Handles updating regular drug schedules.
/// </summary>
internal sealed class UpdateRegularDrugHandler(
    IRegularDrugsApplicationService _regularDrugsApplicationService)
    : IRequestHandler<UpdateRegularDrugCommand, ErrorOr<UpdateRegularDrugResponse>>
{
    public async Task<ErrorOr<UpdateRegularDrugResponse>> Handle(
        UpdateRegularDrugCommand command,
        CancellationToken cancellationToken)
    {
        ErrorOr<Shared.RegularDrugListItemDto> updated = await _regularDrugsApplicationService.UpdateScheduleAsync(
            command.UserId,
            command.Id,
            command.Request,
            cancellationToken);
        if (updated.IsError)
        {
            return updated.Errors;
        }

        return updated.Value.MapToResponse();
    }
}
