using DrugsApp.Domain.PatientSupervisionFeature.Enums;

namespace DrugsApp.Domain.PatientSupervisionFeature;

/// <summary>
/// Canonical API tokens for medical change proposal categories and sources.
/// </summary>
public static class MedicalChangeProposalCategoryCatalog
{
    /// <summary>
    /// Attempts to parse a category token into a <see cref="MedicalChangeProposalCategory"/>.
    /// </summary>
    /// <param name="token">Category token from API input.</param>
    /// <param name="category">Parsed category when successful.</param>
    /// <returns>True when the token resolves to a known category.</returns>
    public static bool TryParseCategory(string? token, out MedicalChangeProposalCategory category)
    {
        category = default;
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        string normalized = token.Trim().ToLowerInvariant();
        return normalized switch
        {
            "women_specific" => Assign(MedicalChangeProposalCategory.WomenSpecific, out category),
            "regular_drug" => Assign(MedicalChangeProposalCategory.RegularDrug, out category),
            "regular_supplement" => Assign(MedicalChangeProposalCategory.RegularSupplement, out category),
            "regular_food" => Assign(MedicalChangeProposalCategory.RegularFood, out category),
            "health_condition" => Assign(MedicalChangeProposalCategory.HealthCondition, out category),
            "drug_problem" => Assign(MedicalChangeProposalCategory.DrugProblem, out category),
            "allergy_drug" => Assign(MedicalChangeProposalCategory.AllergyDrug, out category),
            "allergy_supplement" => Assign(MedicalChangeProposalCategory.AllergySupplement, out category),
            "allergy_food" => Assign(MedicalChangeProposalCategory.AllergyFood, out category),
            "allergy_other" => Assign(MedicalChangeProposalCategory.AllergyOther, out category),
            _ => Enum.TryParse(token, ignoreCase: true, out category)
        };
    }

    /// <summary>
    /// Attempts to parse a source token into a <see cref="MedicalChangeProposalSource"/>.
    /// </summary>
    /// <param name="token">Source token from API input.</param>
    /// <param name="source">Parsed source when successful.</param>
    /// <returns>True when the token resolves to a known source.</returns>
    public static bool TryParseSource(string? token, out MedicalChangeProposalSource source)
    {
        source = default;
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        string normalized = token.Trim().ToLowerInvariant();
        return normalized switch
        {
            "database" => Assign(MedicalChangeProposalSource.Database, out source),
            "custom" => Assign(MedicalChangeProposalSource.Custom, out source),
            "profile_data" => Assign(MedicalChangeProposalSource.ProfileData, out source),
            _ => Enum.TryParse(token, ignoreCase: true, out source)
        };
    }

    private static bool Assign<T>(T value, out T result)
    {
        result = value;
        return true;
    }

    /// <summary>
    /// Maps a category enum value to its API token.
    /// </summary>
    /// <param name="category">Category to convert.</param>
    /// <returns>API token string.</returns>
    public static string ToCategoryToken(MedicalChangeProposalCategory category) =>
        category switch
        {
            MedicalChangeProposalCategory.WomenSpecific => "women_specific",
            MedicalChangeProposalCategory.RegularDrug => "regular_drug",
            MedicalChangeProposalCategory.RegularSupplement => "regular_supplement",
            MedicalChangeProposalCategory.RegularFood => "regular_food",
            MedicalChangeProposalCategory.HealthCondition => "health_condition",
            MedicalChangeProposalCategory.DrugProblem => "drug_problem",
            MedicalChangeProposalCategory.AllergyDrug => "allergy_drug",
            MedicalChangeProposalCategory.AllergySupplement => "allergy_supplement",
            MedicalChangeProposalCategory.AllergyFood => "allergy_food",
            MedicalChangeProposalCategory.AllergyOther => "allergy_other",
            _ => category.ToString()
        };

    /// <summary>
    /// Maps a source enum value to its API token.
    /// </summary>
    /// <param name="source">Source to convert.</param>
    /// <returns>API token string.</returns>
    public static string ToSourceToken(MedicalChangeProposalSource source) =>
        source switch
        {
            MedicalChangeProposalSource.Database => "database",
            MedicalChangeProposalSource.Custom => "custom",
            MedicalChangeProposalSource.ProfileData => "profile_data",
            _ => source.ToString().ToLowerInvariant()
        };
}
