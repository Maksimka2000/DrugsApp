using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Features.ActiveSubstanceFeature.Shared;

namespace DrugsApp.Features.ActiveSubstanceFeature.AddActiveSubstance;

/// <summary>
/// Mapping extensions for AddActiveSubstance use case.
/// </summary>
public static class AddActiveSubstanceMapping
{
    /// <summary>
    /// Maps add request to AddActiveSubstanceCommand.
    /// </summary>
    public static AddActiveSubstanceCommand MapToCommand(this AddActiveSubstanceRequest request) => new(request);

    /// <summary>
    /// Maps AddActiveSubstanceCommand to ActiveSubstance aggregate.
    /// </summary>
    public static ActiveSubstance MapToEntity(this AddActiveSubstanceCommand command)
    {
        var activeSubstance = ActiveSubstance.Create(command.Request.ActiveSubstanceName);

        if (command.Request.OralMedicationIntakeGuidelines is not null)
        {
            AddOralMedicationIntakeGuidelinesRequest request = command.Request.OralMedicationIntakeGuidelines;

            activeSubstance.InitializeOralMedicationIntakeGuidelines(
                foodIntakeType: request.FoodIntakeType,
                waitBeforeMinutes: request.WaitBeforeMinutes,
                waitAfterMinutes: request.WaitAfterMinutes,
                mealNotes: request.MealNotes,
                avoidFoodDrug: request.AvoidFoodDrug,
                avoidFoodTherapy: request.AvoidFoodTherapy,
                liquidNotes: request.LiquidNotes,
                liquidInstructions: request.LiquidInstructions,
                recommendedMorning: request.RecommendedMorning,
                recommendedEvening: request.RecommendedEvening);
        }

        return activeSubstance;
    }

    /// <summary>
    /// Maps ActiveSubstance aggregate to CreatedActiveSubstanceResponse.
    /// </summary>
    public static CreatedActiveSubstanceResponse MapToResponse(this ActiveSubstance activeSubstance) => new(
        Id: activeSubstance.Id,
        ActiveSubstanceName: activeSubstance.ActiveSubstanceName,
        OralMedicationIntakeGuidelines: activeSubstance.OralMedicationIntakeGuidelines?.MapToDto());
}


