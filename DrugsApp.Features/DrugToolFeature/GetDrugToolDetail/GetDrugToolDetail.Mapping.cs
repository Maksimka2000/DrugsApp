using DrugsApp.Features.DrugToolFeature.Shared;

namespace DrugsApp.Features.DrugToolFeature.GetDrugToolDetail;

internal static class GetDrugToolDetailMapping
{
    public static GetDrugToolDetailQuery MapToQuery(
        Guid drugId,
        Guid? selectedDrugFormId,
        string interactionScope,
        Guid userId) =>
        new(
            drugId,
            selectedDrugFormId,
            interactionScope == "user_only"
                ? DrugToolInteractionScope.UserOnly
                : DrugToolInteractionScope.All,
            userId);
}
