using DrugsApp.Features.DrugToolFeature.ApplicationServices;
using DrugsApp.Features.DrugToolFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.DrugToolFeature.GetDrugToolDetail;

internal sealed class GetDrugToolDetailHandler(
    IDrugToolDetailApplicationService _drugToolDetailApplicationService)
    : IRequestHandler<GetDrugToolDetailQuery, ErrorOr<GetDrugToolDetailResponse>>
{
    public async Task<ErrorOr<GetDrugToolDetailResponse>> Handle(
        GetDrugToolDetailQuery query,
        CancellationToken cancellationToken)
    {
        if (query.UserId == Guid.Empty)
        {
            return Error.Unauthorized(
                description: "Authenticated user context is required.",
                metadata: new Dictionary<string, object> { ["reason"] = "missingUserId" });
        }

        ErrorOr<DrugToolDetailBuildResult> buildResult = await _drugToolDetailApplicationService.BuildAsync(
            query.DrugId,
            query.SelectedDrugFormId,
            query.InteractionScope,
            query.UserId,
            cancellationToken);

        if (buildResult.IsError)
        {
            return buildResult.Errors;
        }

        return DrugToolMappings.MapToResponse(buildResult.Value);
    }
}
