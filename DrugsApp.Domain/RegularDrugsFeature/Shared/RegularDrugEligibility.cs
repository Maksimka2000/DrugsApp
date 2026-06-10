using System;
using DrugsApp.Domain.DrugFeature.Entities;

namespace DrugsApp.Domain.RegularDrugsFeature.Shared;

/// <summary>
/// Eligibility rules for catalog drugs referenced by regular drug schedules.
/// </summary>
public static class RegularDrugEligibility
{
    /// <summary>
    /// Determines whether a catalog drug may be added to a user's regular drug list.
    /// </summary>
    /// <param name="drug">Catalog drug aggregate.</param>
    /// <returns>True when the drug is eligible for regular intake scheduling.</returns>
    public static bool IsEligibleDrug(Drug drug) =>
        drug.TypeOfDrug == DrugType.Standard &&
        !drug.TypeOfProduct.Equals("Weterynaryjny", StringComparison.OrdinalIgnoreCase);
}
