using System;

namespace DrugsApp.Domain.DrugProblemsFeature.Enums;

/// <summary>
/// Indicates whether a drug problem references a catalog drug or stores a custom user-entered name.
/// </summary>
public enum DrugProblemSource
{
    /// <summary>
    /// Catalog-backed drug problem associated with a specific drug.
    /// </summary>
    Database = 0,

    /// <summary>
    /// Custom drug problem entered by the user without catalog linkage.
    /// </summary>
    Custom = 1,
}
