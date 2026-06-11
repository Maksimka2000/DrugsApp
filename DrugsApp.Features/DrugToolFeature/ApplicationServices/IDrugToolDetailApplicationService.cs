using DrugsApp.Features.DrugToolFeature.Shared;
using ErrorOr;

namespace DrugsApp.Features.DrugToolFeature.ApplicationServices;

/// <summary>
/// Internal build result for the drug tool detail response [description].
/// </summary>
public sealed record DrugToolDetailBuildResult(
    Guid DrugId,
    string CommercialName,
    string TypeOfProduct,
    DrugsApp.Domain.DrugFeature.Entities.DrugType TypeOfDrug,
    Guid SelectedDrugFormId,
    DrugToolFormSummaryDto SelectedDrugFormSummary,
    string DrugDisplayLine,
    IReadOnlyCollection<DrugToolActiveSubstanceDto> ActiveSubstances,
    IReadOnlyCollection<DrugToolAvailableDrugFormDto> AvailableDrugForms,
    DrugToolInteractionScope InteractionScope,
    IReadOnlyCollection<DrugToolUserMealTimeDto> UserMealTimes,
    DrugToolAlcoholSectionDto AlcoholSection,
    DrugToolDrugInteractionSectionDto DrugSection,
    DrugToolFoodInteractionSectionDto FoodSection,
    DrugToolSupplementInteractionSectionDto SupplementSection);

/// <summary>
/// Application service contract for assembling the drug tool detail page [description].
/// </summary>
public interface IDrugToolDetailApplicationService
{
    /// <summary>
    /// Builds the aggregated drug tool detail projection for one selected drug form.
    /// </summary>
    /// <param name="drugId">Catalog drug identifier.</param>
    /// <param name="selectedDrugFormId">Optional selected form identifier.</param>
    /// <param name="interactionScope">Interaction scope filter.</param>
    /// <param name="userId">Authenticated client user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Aggregated detail result or business errors.</returns>
    Task<ErrorOr<DrugToolDetailBuildResult>> BuildAsync(
        Guid drugId,
        Guid? selectedDrugFormId,
        DrugToolInteractionScope interactionScope,
        Guid userId,
        CancellationToken cancellationToken);
}
