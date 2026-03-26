using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.ActiveSubstanceFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.ActiveSubstanceFeature.AddOralMedicationIntakeGuidelines;

/// <summary>
/// Handles attaching oral medication intake guidelines to an active substance.
/// </summary>
internal sealed class AddOralMedicationIntakeGuidelinesHandler(IActiveSubstanceRepository _activeSubstanceRepository)
        : IRequestHandler<AddOralMedicationIntakeGuidelinesCommand, ErrorOr<CreatedOralMedicationIntakeGuidelinesResponse>>
{

    /// <summary>
    /// Attaches oral medication intake guidelines if none exist.
    /// </summary>
    /// <param name="command">Create command containing the active substance id and guidelines payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created oral medication intake guidelines response.</returns>
    public async Task<ErrorOr<CreatedOralMedicationIntakeGuidelinesResponse>> Handle(
        AddOralMedicationIntakeGuidelinesCommand command,
        CancellationToken cancellationToken)
    {
        ActiveSubstance? activeSubstance = await _activeSubstanceRepository.GetByIdAsync(
            command.ActiveSubstanceId,
            cancellationToken);

        if (activeSubstance is null)
        {
            return Error.NotFound(
                description: "The requested active substance was not found.",
                metadata: new Dictionary<string, object>
                {
                    ["resourceType"] = "ActiveSubstance",
                    ["resourceId"] = command.ActiveSubstanceId.ToString()
                });
        }

        if (activeSubstance.OralMedicationIntakeGuidelines is not null)
        {
            return Error.Conflict(
                description: "Oral medication intake guidelines already exist for this active substance.",
                metadata: new Dictionary<string, object>
                {
                    ["conflictReason"] = "oralMedicationIntakeGuidelinesAlreadyExists",
                    ["resourceId"] = activeSubstance.Id.ToString()
                });
        }

        activeSubstance.InitializeOralMedicationIntakeGuidelines(
            foodIntakeType: command.Request.FoodIntakeType,
            waitBeforeMinutes: command.Request.WaitBeforeMinutes,
            waitAfterMinutes: command.Request.WaitAfterMinutes,
            mealNotes: command.Request.MealNotes,
            avoidFoodDrug: command.Request.AvoidFoodDrug,
            avoidFoodTherapy: command.Request.AvoidFoodTherapy,
            liquidNotes: command.Request.LiquidNotes,
            liquidInstructions: command.Request.LiquidInstructions,
            recommendedMorning: command.Request.RecommendedMorning,
            recommendedEvening: command.Request.RecommendedEvening );

        OralMedicationIntakeGuidelines guidelines = activeSubstance.OralMedicationIntakeGuidelines!;

        return guidelines.MapToCreatedResponse(
            activeSubstanceId: activeSubstance.Id);
    }
}


