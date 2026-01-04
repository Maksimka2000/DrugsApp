using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.ActiveSubstanceFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.ActiveSubstanceFeature.UpdateActiveSubstance;

/// <summary>
/// Handles partially updating an active substance and its oral guidelines.
/// </summary>
internal sealed class UpdateActiveSubstanceHandler(IActiveSubstanceRepository _activeSubstanceRepository)
    : IRequestHandler<UpdateActiveSubstanceCommand, ErrorOr<UpdatedActiveSubstanceResponse>>
{

    /// <summary>
    /// Applies a partial update to an active substance and its oral guidelines.
    /// </summary>
    /// <param name="command">Update command containing the active substance id and request payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated active substance response.</returns>
    public async Task<ErrorOr<UpdatedActiveSubstanceResponse>> Handle(
        UpdateActiveSubstanceCommand command,
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

        if (command.Request.ActiveSubstanceName is not null)
        {
            string normalizedName = command.Request.ActiveSubstanceName.Trim();

            if (normalizedName != activeSubstance.ActiveSubstanceName)
            {
                ActiveSubstance? duplicate = await _activeSubstanceRepository.GetByNameAsync(
                    normalizedName,
                    cancellationToken);

                if (duplicate is not null && duplicate.Id != activeSubstance.Id)
                {
                    return Error.Conflict(
                        description: "An active substance with the same name already exists.",
                        metadata: new Dictionary<string, object>
                        {
                            ["conflictReason"] = "duplicateActiveSubstanceName",
                            ["resourceId"] = duplicate.Id.ToString()
                        });
                }

                activeSubstance.UpdateActiveSubstanceName(normalizedName);
            }
        }


        if (activeSubstance.OralMedicationIntakeGuidelines is not null)
        {
            OralMedicationIntakeGuidelines guidelines = activeSubstance.OralMedicationIntakeGuidelines;

            if (command.Request.FoodIntakeType is not null)
            {
                guidelines.UpdateFoodIntakeType(command.Request.FoodIntakeType.Value);
            }

            if (command.Request.ClearWaitBeforeMinutes == true)
            {
                guidelines.SetWaitBeforeMinutes(null);
            }
            else if (command.Request.WaitBeforeMinutes is not null)
            {
                guidelines.SetWaitBeforeMinutes(command.Request.WaitBeforeMinutes.Value);
            }

            if (command.Request.ClearWaitAfterMinutes == true)
            {
                guidelines.SetWaitAfterMinutes(null);
            }
            else if (command.Request.WaitAfterMinutes is not null)
            {
                guidelines.SetWaitAfterMinutes(command.Request.WaitAfterMinutes.Value);
            }

            if (command.Request.ClearMealNotes == true)
            {
                guidelines.SetMealNotes(null);
            }
            else if (!string.IsNullOrWhiteSpace(command.Request.MealNotes))
            {
                guidelines.SetMealNotes(command.Request.MealNotes);
            }

            if (command.Request.ClearAvoidFoodDrug == true)
            {
                guidelines.SetAvoidFoodDrug(null);
            }
            else if (!string.IsNullOrWhiteSpace(command.Request.AvoidFoodDrug))
            {
                guidelines.SetAvoidFoodDrug(command.Request.AvoidFoodDrug);
            }

            if (command.Request.ClearAvoidFoodTherapy == true)
            {
                guidelines.SetAvoidFoodTherapy(null);
            }
            else if (!string.IsNullOrWhiteSpace(command.Request.AvoidFoodTherapy))
            {
                guidelines.SetAvoidFoodTherapy(command.Request.AvoidFoodTherapy);
            }

            if (command.Request.ClearLiquidNotes == true)
            {
                guidelines.SetLiquidNotes(null);
            }
            else if (!string.IsNullOrWhiteSpace(command.Request.LiquidNotes))
            {
                guidelines.SetLiquidNotes(command.Request.LiquidNotes);
            }

            if (command.Request.ClearLiquidInstructions == true)
            {
                guidelines.SetLiquidInstructions(null);
            }
            else if (command.Request.LiquidInstructions is not null)
            {
                guidelines.SetLiquidInstructions(command.Request.LiquidInstructions.Value);
            }

            if (command.Request.ClearRecommendedMorning == true)
            {
                guidelines.SetRecommendedMorning(null);
            }
            else if (command.Request.RecommendedMorning is not null)
            {
                guidelines.SetRecommendedMorning(command.Request.RecommendedMorning.Value);
            }

            if (command.Request.ClearRecommendedEvening == true)
            {
                guidelines.SetRecommendedEvening(null);
            }
            else if (command.Request.RecommendedEvening is not null)
            {
                guidelines.SetRecommendedEvening(command.Request.RecommendedEvening.Value);
            }
        }

        return activeSubstance.MapToUpdatedResponse();
    }
}


