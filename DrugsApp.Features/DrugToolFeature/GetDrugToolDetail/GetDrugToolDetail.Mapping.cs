using DrugsApp.Features.DrugToolFeature.Shared;

namespace DrugsApp.Features.DrugToolFeature.GetDrugToolDetail;

internal static class GetDrugToolDetailMapping
{
    public static GetDrugToolDetailQuery MapToQuery(
        Guid drugFormId,
        string interactionScope,
        Guid userId) =>
        new(
            drugFormId,
            interactionScope == "user_only"
                ? DrugToolInteractionScope.UserOnly
                : DrugToolInteractionScope.All,
            userId);
}
