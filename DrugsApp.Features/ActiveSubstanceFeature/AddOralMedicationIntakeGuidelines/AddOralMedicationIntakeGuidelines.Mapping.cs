using System;
using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Features.ActiveSubstanceFeature.Shared;

namespace DrugsApp.Features.ActiveSubstanceFeature.AddOralMedicationIntakeGuidelines;

/// <summary>
/// Mapping extensions for AddOralMedicationIntakeGuidelines use case.
/// </summary>
public static class AddOralMedicationIntakeGuidelinesMapping
{
    /// <summary>
    /// Maps add request and route parameter to AddOralMedicationIntakeGuidelinesCommand.
    /// </summary>
    public static AddOralMedicationIntakeGuidelinesCommand MapToCommand(
        this AddOralMedicationIntakeGuidelinesRequest request,
        Guid activeSubstanceId) => new(
        ActiveSubstanceId: activeSubstanceId,
        Request: request);

    /// <summary>
    /// Maps created entity state to CreatedOralMedicationIntakeGuidelinesResponse.
    /// </summary>
    public static CreatedOralMedicationIntakeGuidelinesResponse MapToCreatedResponse(
        this OralMedicationIntakeGuidelines guidelines,
        Guid activeSubstanceId) => new(
        ActiveSubstanceId: activeSubstanceId,
        OralMedicationIntakeGuidelines: guidelines.MapToDto());
}


