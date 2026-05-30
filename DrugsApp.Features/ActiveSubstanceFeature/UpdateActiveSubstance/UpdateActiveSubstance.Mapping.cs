using System;
using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Features.ActiveSubstanceFeature.Shared;

namespace DrugsApp.Features.ActiveSubstanceFeature.UpdateActiveSubstance;

/// <summary>
/// Mapping extensions for UpdateActiveSubstance use case.
/// </summary>
public static class UpdateActiveSubstanceMapping
{
    /// <summary>
    /// Maps update request and route parameter to UpdateActiveSubstanceCommand.
    /// </summary>
    public static UpdateActiveSubstanceCommand MapToCommand(
        this UpdateActiveSubstanceRequest request,
        Guid activeSubstanceId) => new(
        ActiveSubstanceId: activeSubstanceId,
        Request: request);

    /// <summary>
    /// Maps ActiveSubstance aggregate to UpdatedActiveSubstanceResponse.
    /// </summary>
    public static UpdatedActiveSubstanceResponse MapToUpdatedResponse(this ActiveSubstance activeSubstance) => new(
        Id: activeSubstance.Id,
        ActiveSubstanceName: activeSubstance.ActiveSubstanceName,
        HasAlcoholInteraction: activeSubstance.HasAlcoholInteraction,
        AlcoholSpecialText: activeSubstance.AlcoholInteractionOverride?.AlcoholSpecialText,
        OralMedicationIntakeGuidelines: activeSubstance.OralMedicationIntakeGuidelines?.MapToDto());
}


