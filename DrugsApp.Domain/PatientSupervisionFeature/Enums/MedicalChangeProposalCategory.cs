namespace DrugsApp.Domain.PatientSupervisionFeature.Enums;

/// <summary>
/// Target domain category for proposal add-only items.
/// </summary>
public enum MedicalChangeProposalCategory
{
    /// <summary>
    /// Women-specific health context.
    /// </summary>
    WomenSpecific,

    /// <summary>
    /// Regular drug regimen.
    /// </summary>
    RegularDrug,

    /// <summary>
    /// Regular supplement regimen.
    /// </summary>
    RegularSupplement,

    /// <summary>
    /// Regular food regimen.
    /// </summary>
    RegularFood,

    /// <summary>
    /// Health condition related.
    /// </summary>
    HealthCondition,

    /// <summary>
    /// Drug problem related.
    /// </summary>
    DrugProblem,

    /// <summary>
    /// Drug allergy related.
    /// </summary>
    AllergyDrug,

    /// <summary>
    /// Supplement allergy related.
    /// </summary>
    AllergySupplement,

    /// <summary>
    /// Food allergy related.
    /// </summary>
    AllergyFood,

    /// <summary>
    /// Other allergy related.
    /// </summary>
    AllergyOther
}
