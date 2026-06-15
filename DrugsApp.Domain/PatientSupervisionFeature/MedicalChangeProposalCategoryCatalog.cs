using DrugsApp.Domain.PatientSupervisionFeature.Enums;

namespace DrugsApp.Domain.PatientSupervisionFeature;

/// <summary>
/// Canonical API tokens for medical change proposal categories and sources.
/// </summary>
public static class MedicalChangeProposalCategoryCatalog
{
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

    public static string ToSourceToken(MedicalChangeProposalSource source) =>
        source switch
        {
            MedicalChangeProposalSource.Database => "database",
            MedicalChangeProposalSource.Custom => "custom",
            MedicalChangeProposalSource.ProfileData => "profile_data",
            _ => source.ToString().ToLowerInvariant()
        };
}
