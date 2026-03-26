using System;


namespace DrugsApp.Features.VetDrugFeature.Shared;

/// <summary>
/// Data transfer object representing a veterinary drug.
/// </summary>
public sealed record VetDrugDto(
    Guid Id,
    int ProductIdentifier,
    string CommercialName,
    string TypeOfProduct);

