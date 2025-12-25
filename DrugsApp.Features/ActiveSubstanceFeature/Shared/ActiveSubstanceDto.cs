using System;

namespace DrugsApp.Features.ActiveSubstanceFeature.Shared;

/// <summary>
/// Data transfer object representing an active substance.
/// </summary>
public sealed record ActiveSubstanceDto(
    Guid Id,
    string ActiveSubstanceName,
    OralMedicationIntakeGuidelinesDto? OralMedicationIntakeGuidelines);


