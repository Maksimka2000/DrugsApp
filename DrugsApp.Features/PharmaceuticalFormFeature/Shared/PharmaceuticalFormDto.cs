using System;

namespace DrugsApp.Features.PharmaceuticalFormFeature.Shared;

/// <summary>
/// Data transfer object representing a pharmaceutical form.
/// </summary>
public sealed record PharmaceuticalFormDto(
    Guid Id,
    string FormulationName);


