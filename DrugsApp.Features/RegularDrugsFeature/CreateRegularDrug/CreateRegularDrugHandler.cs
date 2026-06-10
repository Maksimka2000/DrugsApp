using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.RegularDrugsFeature.ApplicationServices;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.RegularDrugsFeature.CreateRegularDrug;

/// <summary>
/// Handles creation of regular drug schedules.
/// </summary>
internal sealed class CreateRegularDrugHandler(
    IRegularDrugsApplicationService _regularDrugsApplicationService)
    : IRequestHandler<CreateRegularDrugCommand, ErrorOr<CreateRegularDrugResponse>>
{
    public async Task<ErrorOr<CreateRegularDrugResponse>> Handle(
        CreateRegularDrugCommand command,
        CancellationToken cancellationToken)
    {
        ErrorOr<Shared.RegularDrugListItemDto> created = await _regularDrugsApplicationService.CreateAsync(
            command.UserId,
            command.Request,
            cancellationToken);
        if (created.IsError)
        {
            return created.Errors;
        }

        return created.Value.MapToResponse();
    }
}
