using DrugsApp.Domain.AllergiesFeature.Entities;
using DrugsApp.Domain.AllergiesFeature.Enums;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.DrugProblemsFeature.Entities;
using DrugsApp.Domain.DrugProblemsFeature.Enums;
using DrugsApp.Domain.HealthConditionFeature.Entities;
using DrugsApp.Domain.RegularDrugsFeature.Entities;
using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Entities;

namespace DrugsApp.Features.DrugInteractionToolFeature.Shared;

/// <summary>
/// Aggregated user-owned medical data loaded once per report and reused across every downstream builder.
/// </summary>
internal sealed record UserMedicalBundle(
    List<UserHealthCondition> HealthConditions,
    List<DrugProblem> DrugProblems,
    List<ClientDrugAllergy> DrugAllergies,
    List<ClientFoodAllergy> FoodAllergies,
    List<ClientSupplementAllergy> SupplementAllergies,
    List<ClientOtherAllergy> OtherAllergies,
    ClientAllergySummary? AllergySummary,
    ClientReproductiveHealthProfile? ReproductiveHealth);

/// <summary>
/// Builds the user information section of the report from already-loaded medical data [description].
/// </summary>
internal static class UserInfoSectionBuilder
{
    internal static UserInfoSectionReadModel BuildUserInfoSection(
        UserMedicalBundle bundle,
        IReadOnlyCollection<ClientRegularDrug> regularDrugs,
        IReadOnlyCollection<DrugFormIntake> plannedDrugs,
        HashSet<Guid> takenNowFoodIds,
        HashSet<Guid> takenNowSupplementIds,
        IReadOnlyDictionary<Guid, IReadOnlyList<OwnSupplementSource>> ownSupplementSourcesBySupplementId,
        IReadOnlySet<Guid>? selectedFoodIds = null,
        IReadOnlySet<Guid>? selectedSupplementIds = null,
        bool includeFoodAndSupplementAllergies = true)
    {
        List<MainWarningReadModel> mainWarnings = BuildMainWarnings(bundle.ReproductiveHealth);
        HealthConditionsGroupReadModel healthConditions = BuildHealthConditionsGroup(bundle.HealthConditions);
        ProblematicDrugsGroupReadModel problematicDrugs = BuildProblematicDrugsGroup(
            bundle.DrugProblems,
            regularDrugs,
            plannedDrugs);
        AllergiesGroupReadModel allergies = BuildAllergiesGroup(
            bundle,
            regularDrugs,
            plannedDrugs,
            takenNowFoodIds,
            takenNowSupplementIds,
            ownSupplementSourcesBySupplementId,
            selectedFoodIds ?? new HashSet<Guid>(),
            selectedSupplementIds ?? new HashSet<Guid>(),
            includeFoodAndSupplementAllergies);

        return new UserInfoSectionReadModel(
            MainWarnings: mainWarnings,
            HealthConditions: healthConditions,
            ProblematicDrugs: problematicDrugs,
            Allergies: allergies);
    }

    private static List<MainWarningReadModel> BuildMainWarnings(ClientReproductiveHealthProfile? profile)
    {
        if (profile is null)
        {
            return new List<MainWarningReadModel>();
        }

        List<MainWarningReadModel> warnings = new();

        if (profile.CurrentlyPregnant)
        {
            warnings.Add(new MainWarningReadModel(
                WarningCode: "currently_pregnant",
                DefaultText: DrugInteractionToolHeadlines.MainWarningCurrentlyPregnant));
        }

        if (profile.PlanningToGetPregnant)
        {
            warnings.Add(new MainWarningReadModel(
                WarningCode: "planning_to_get_pregnant",
                DefaultText: DrugInteractionToolHeadlines.MainWarningPlanningPregnancy));
        }

        return warnings;
    }

    private static HealthConditionsGroupReadModel BuildHealthConditionsGroup(List<UserHealthCondition> rows)
    {
        var databaseRows = rows
            .Where(row => row.Source == HealthConditionSource.Database && row.HealthCondition is not null)
            .Select(row => new HealthConditionRowReadModel(
                HealthConditionId: row.HealthConditionId!.Value,
                HealthConditionName: row.HealthCondition!.HealthConditionName,
                UserNote: row.Note,
                DisplayLine: $"{row.HealthCondition.HealthConditionName} | {row.Note}"))
            .ToList();

        var customRows = rows
            .Where(row => row.Source == HealthConditionSource.Custom)
            .Select(row => new CustomHealthConditionRowReadModel(
                CustomName: row.CustomHealthConditionName ?? string.Empty,
                UserNote: row.Note,
                DisplayLine: $"{row.CustomHealthConditionName} | {row.Note}"))
            .ToList();

        return new HealthConditionsGroupReadModel(
            FromDatabase: DrugInteractionToolReportMapping.BuildSubsection(
                "health_conditions.from_database",
                DrugInteractionToolHeadlines.HealthConditionsDatabase,
                databaseRows),
            Custom: DrugInteractionToolReportMapping.BuildSubsection(
                "health_conditions.custom",
                DrugInteractionToolHeadlines.HealthConditionsCustom,
                customRows));
    }

    private static ProblematicDrugsGroupReadModel BuildProblematicDrugsGroup(
        List<DrugProblem> problems,
        IReadOnlyCollection<ClientRegularDrug> regularDrugs,
        IReadOnlyCollection<DrugFormIntake> plannedDrugs)
    {
        var databaseRows = problems
            .Where(problem => problem.Source == DrugProblemSource.Database && problem.DrugForm?.Drug is not null)
            .Select(problem =>
            {
                Drug drug = problem.DrugForm!.Drug;
                DrugForm drugForm = problem.DrugForm;
                string drugSummaryText = DrugInteractionToolReportMapping.BuildDrugSummaryText(drug, drugForm);

                return new ProblematicDrugRowReadModel(
                    DrugId: drug.Id,
                    DrugFormId: drugForm.Id,
                    DrugSummaryText: drugSummaryText,
                    UserNote: problem.ProblemNote,
                    DisplayLine: $"{drugSummaryText} | {problem.ProblemNote}");
            })
            .ToList();

        var customRows = problems
            .Where(problem => problem.Source == DrugProblemSource.Custom)
            .Select(problem =>
            {
                string? otherCustomData = DrugInteractionToolReportMapping.BuildOtherCustomData(
                    problem.CustomActiveSubstances,
                    problem.CustomDosage,
                    problem.CustomFormulationName);
                string customCommercialName = problem.CustomDrugName ?? string.Empty;
                string displayLine = string.Join(
                    " | ",
                    new[] { customCommercialName, otherCustomData, problem.ProblemNote }
                        .Where(part => !string.IsNullOrWhiteSpace(part)));

                return new CustomProblematicDrugRowReadModel(
                    CustomCommercialName: customCommercialName,
                    OtherCustomData: string.IsNullOrWhiteSpace(otherCustomData) ? null : otherCustomData,
                    UserNote: problem.ProblemNote,
                    DisplayLine: displayLine);
            })
            .ToList();

        return new ProblematicDrugsGroupReadModel(
            FromDatabase: DrugInteractionToolReportMapping.BuildSubsection(
                "problematic_drugs.from_database",
                DrugInteractionToolHeadlines.ProblematicDrugsDatabase,
                databaseRows),
            Custom: DrugInteractionToolReportMapping.BuildSubsection(
                "problematic_drugs.custom",
                DrugInteractionToolHeadlines.ProblematicDrugsCustom,
                customRows),
            TakenNowOverlap: DrugTakenNowProblemOverlapBuilder.BuildSubsection(regularDrugs, problems),
            PlanTakingOverlap: DrugTakenNowProblemOverlapBuilder.BuildPlanTakingSubsection(plannedDrugs, problems));
    }

    private static AllergiesGroupReadModel BuildAllergiesGroup(
        UserMedicalBundle bundle,
        IReadOnlyCollection<ClientRegularDrug> regularDrugs,
        IReadOnlyCollection<DrugFormIntake> plannedDrugs,
        HashSet<Guid> takenFoodIds,
        HashSet<Guid> takenSupplementIds,
        IReadOnlyDictionary<Guid, IReadOnlyList<OwnSupplementSource>> ownSupplementSources,
        IReadOnlySet<Guid> selectedFoodIds,
        IReadOnlySet<Guid> selectedSupplementIds,
        bool includeFoodAndSupplementAllergies)
    {
        return new AllergiesGroupReadModel(
            DrugAllergies: BuildDrugAllergySubgroup(bundle.DrugAllergies, regularDrugs, plannedDrugs),
            SupplementAllergies: includeFoodAndSupplementAllergies
                ? BuildSupplementAllergySubgroup(
                    bundle.SupplementAllergies,
                    takenSupplementIds,
                    ownSupplementSources,
                    selectedSupplementIds)
                : null,
            FoodAllergies: includeFoodAndSupplementAllergies
                ? BuildFoodAllergySubgroup(bundle.FoodAllergies, takenFoodIds, selectedFoodIds)
                : null,
            OtherAllergies: BuildOtherAllergySubgroup(bundle.AllergySummary, bundle.OtherAllergies));
    }

    private static AllergyDrugSubgroupReadModel BuildDrugAllergySubgroup(
        IReadOnlyCollection<ClientDrugAllergy> allergies,
        IReadOnlyCollection<ClientRegularDrug> regularDrugs,
        IReadOnlyCollection<DrugFormIntake> plannedDrugs)
    {
        var databaseRows = allergies
            .Where(allergy => allergy.Source == AllergyRecordSource.Database && allergy.DrugForm?.Drug is not null)
            .Select(allergy =>
            {
                Drug drug = allergy.DrugForm!.Drug;
                DrugForm drugForm = allergy.DrugForm;
                string drugSummaryText = DrugInteractionToolReportMapping.BuildDrugSummaryText(drug, drugForm);

                return new DrugAllergyRowReadModel(
                    DrugId: drug.Id,
                    DrugFormId: drugForm.Id,
                    DrugSummaryText: drugSummaryText,
                    UserNote: allergy.Note,
                    DisplayLine: $"{drugSummaryText} | {allergy.Note}");
            })
            .ToList();

        var customRows = allergies
            .Where(allergy => allergy.Source == AllergyRecordSource.Custom)
            .Select(allergy =>
            {
                string? otherCustomData = DrugInteractionToolReportMapping.BuildOtherCustomData(
                    allergy.CustomActiveSubstances,
                    allergy.CustomDosage,
                    allergy.CustomFormulationName);
                string customCommercialName = allergy.CustomDrugName ?? string.Empty;
                string displayLine = string.Join(
                    " | ",
                    new[] { customCommercialName, otherCustomData, allergy.Note }
                        .Where(part => !string.IsNullOrWhiteSpace(part)));

                return new CustomDrugAllergyRowReadModel(
                    CustomCommercialName: customCommercialName,
                    OtherCustomData: string.IsNullOrWhiteSpace(otherCustomData) ? null : otherCustomData,
                    UserNote: allergy.Note,
                    DisplayLine: displayLine);
            })
            .ToList();

        return new AllergyDrugSubgroupReadModel(
            FromDatabase: DrugInteractionToolReportMapping.BuildSubsection(
                "allergies.drug.from_database",
                DrugInteractionToolHeadlines.DrugAllergiesDatabase,
                databaseRows),
            Custom: DrugInteractionToolReportMapping.BuildSubsection(
                "allergies.drug.custom",
                DrugInteractionToolHeadlines.DrugAllergiesCustom,
                customRows),
            TakenNowOverlap: DrugTakenNowAllergyOverlapBuilder.BuildSubsection(regularDrugs, allergies),
            PlanTakingOverlap: DrugTakenNowAllergyOverlapBuilder.BuildPlanTakingSubsection(plannedDrugs, allergies));
    }

    private static AllergyFoodSubgroupReadModel BuildFoodAllergySubgroup(
        IReadOnlyCollection<ClientFoodAllergy> allergies,
        HashSet<Guid> takenFoodIds,
        IReadOnlySet<Guid> selectedFoodIds)
    {
        var databaseRows = allergies
            .Where(allergy => allergy.Source == AllergyRecordSource.Database && allergy.Food is not null)
            .Select(allergy => new FoodCatalogAllergyRowReadModel(
                FoodId: allergy.Food!.Id,
                Name: allergy.Food.Name,
                AliasNames: allergy.Food.Aliases.ToList(),
                UserNote: allergy.Note,
                DisplayLine: $"{allergy.Food.Name} | {allergy.Note}"))
            .ToList();

        var customRows = allergies
            .Where(allergy => allergy.Source == AllergyRecordSource.Custom)
            .Select(allergy => new CustomFoodAllergyRowReadModel(
                Name: allergy.CustomFoodName ?? string.Empty,
                UserNote: allergy.Note,
                DisplayLine: $"{allergy.CustomFoodName} | {allergy.Note}"))
            .ToList();

        var overlapRows = allergies
            .Where(allergy => allergy.Source == AllergyRecordSource.Database
                && allergy.FoodId.HasValue
                && takenFoodIds.Contains(allergy.FoodId.Value))
            .Select(allergy => new FoodTakenNowAllergyOverlapRowReadModel(
                RegularFoodId: allergy.FoodId!.Value,
                RegularIntakeName: allergy.Food!.Name,
                AliasNames: allergy.Food.Aliases.ToList(),
                AllergyNote: allergy.Note,
                DisplayLine: $"{allergy.Food.Name} | {allergy.Note}"))
            .ToList();

        var planRows = allergies
            .Where(allergy => allergy.Source == AllergyRecordSource.Database
                && allergy.FoodId.HasValue
                && allergy.Food is not null
                && selectedFoodIds.Contains(allergy.FoodId.Value))
            .Select(allergy => new FoodTakenNowAllergyOverlapRowReadModel(
                RegularFoodId: allergy.FoodId!.Value,
                RegularIntakeName: allergy.Food!.Name,
                AliasNames: allergy.Food.Aliases.ToList(),
                AllergyNote: allergy.Note,
                DisplayLine: $"{allergy.Food.Name} | {allergy.Note}"))
            .ToList();

        return new AllergyFoodSubgroupReadModel(
            FromDatabase: DrugInteractionToolReportMapping.BuildSubsection(
                "allergies.food.from_database",
                DrugInteractionToolHeadlines.FoodAllergiesDatabase,
                databaseRows),
            Custom: DrugInteractionToolReportMapping.BuildSubsection(
                "allergies.food.custom",
                DrugInteractionToolHeadlines.FoodAllergiesCustom,
                customRows),
            TakenNowOverlap: DrugInteractionToolReportMapping.BuildSubsection(
                "allergies.food.taken_now_overlap",
                DrugInteractionToolHeadlines.FoodAllergiesTakenNowOverlap,
                overlapRows),
            PlanTakingOverlap: DrugInteractionToolReportMapping.BuildSubsection(
                "allergies.food.plan_taking_overlap",
                DrugInteractionToolHeadlines.FoodAllergiesPlanTakingOverlap,
                planRows));
    }

    private static AllergySupplementSubgroupReadModel BuildSupplementAllergySubgroup(
        IReadOnlyCollection<ClientSupplementAllergy> allergies,
        HashSet<Guid> takenSupplementIds,
        IReadOnlyDictionary<Guid, IReadOnlyList<OwnSupplementSource>> ownSupplementSources,
        IReadOnlySet<Guid> selectedSupplementIds)
    {
        var databaseRows = allergies
            .Where(allergy => allergy.Source == AllergyRecordSource.Database && allergy.Supplement is not null)
            .Select(allergy => new SupplementCatalogAllergyRowReadModel(
                SupplementId: allergy.Supplement!.Id,
                Name: allergy.Supplement.Name,
                AliasNames: allergy.Supplement.Aliases.ToList(),
                UserNote: allergy.Note,
                DisplayLine: $"{allergy.Supplement.Name} | {allergy.Note}"))
            .ToList();

        var customRows = allergies
            .Where(allergy => allergy.Source == AllergyRecordSource.Custom)
            .Select(allergy => new CustomSupplementAllergyRowReadModel(
                Name: allergy.CustomSupplementName ?? string.Empty,
                UserNote: allergy.Note,
                DisplayLine: $"{allergy.CustomSupplementName} | {allergy.Note}"))
            .ToList();

        List<SupplementTakenNowAllergyOverlapRowReadModel> overlapRows = new();
        List<SupplementTakenNowAllergyOverlapRowReadModel> planRows = new();
        List<SupplementTakenNowAllergyOverlapRowReadModel> ownOverlapRows = new();

        foreach (ClientSupplementAllergy allergy in allergies.Where(allergy =>
                     allergy.Source == AllergyRecordSource.Database && allergy.Supplement is not null))
        {
            Guid supplementId = allergy.SupplementId!.Value;
            Supplement supplement = allergy.Supplement!;

            if (takenSupplementIds.Contains(supplementId))
            {
                overlapRows.Add(new SupplementTakenNowAllergyOverlapRowReadModel(
                    RegularSupplementId: supplementId,
                    RegularIntakeName: supplement.Name,
                    AliasNames: supplement.Aliases.ToList(),
                    AllergyNote: allergy.Note,
                    DisplayLine: $"{supplement.Name} | {allergy.Note}"));
            }

            if (selectedSupplementIds.Contains(supplementId))
            {
                planRows.Add(new SupplementTakenNowAllergyOverlapRowReadModel(
                    RegularSupplementId: supplementId,
                    RegularIntakeName: supplement.Name,
                    AliasNames: supplement.Aliases.ToList(),
                    AllergyNote: allergy.Note,
                    DisplayLine: $"{supplement.Name} | {allergy.Note}"));
            }

            if (!ownSupplementSources.TryGetValue(supplementId, out IReadOnlyList<OwnSupplementSource>? sources))
            {
                continue;
            }

            ownOverlapRows.AddRange(sources.Select(source => new SupplementTakenNowAllergyOverlapRowReadModel(
                RegularSupplementId: supplementId,
                RegularIntakeName: supplement.Name,
                AliasNames: supplement.Aliases.ToList(),
                AllergyNote: allergy.Note,
                DisplayLine: $"{supplement.Name} in own supplement ({source.OwnSupplementName}) | {allergy.Note}",
                OwnSupplementId: source.OwnSupplementId,
                OwnSupplementName: source.OwnSupplementName)));
        }

        return new AllergySupplementSubgroupReadModel(
            FromDatabase: DrugInteractionToolReportMapping.BuildSubsection(
                "allergies.supplement.from_database",
                DrugInteractionToolHeadlines.SupplementAllergiesDatabase,
                databaseRows),
            Custom: DrugInteractionToolReportMapping.BuildSubsection(
                "allergies.supplement.custom",
                DrugInteractionToolHeadlines.SupplementAllergiesCustom,
                customRows),
            TakenNowOverlap: DrugInteractionToolReportMapping.BuildSubsection(
                "allergies.supplement.taken_now_overlap",
                DrugInteractionToolHeadlines.SupplementAllergiesTakenNowOverlap,
                overlapRows),
            PlanTakingOverlap: DrugInteractionToolReportMapping.BuildSubsection(
                "allergies.supplement.plan_taking_overlap",
                DrugInteractionToolHeadlines.SupplementAllergiesPlanTakingOverlap,
                planRows),
            OwnSupplementOverlap: DrugInteractionToolReportMapping.BuildSubsection(
                "allergies.supplement.own_supplement_overlap",
                DrugInteractionToolHeadlines.SupplementAllergiesOwnSupplementOverlap,
                ownOverlapRows));
    }

    private static AllergyOtherSubgroupReadModel BuildOtherAllergySubgroup(
        ClientAllergySummary? summary,
        IReadOnlyCollection<ClientOtherAllergy> customAllergies)
    {
        List<OtherAllergyCatalogRowReadModel> databaseRows = new();

        if (summary?.PollenDustMitesAllergyStatus == AllergyAnswerState.Yes)
        {
            databaseRows.Add(new OtherAllergyCatalogRowReadModel(
                Name: "Pollen / dust / mites",
                DescriptionFromDb: "User declared pollen, dust, or mites allergy.",
                DisplayLine: "Pollen / dust / mites allergy declared."));
        }

        if (summary?.InsectVenomAllergyStatus == AllergyAnswerState.Yes)
        {
            databaseRows.Add(new OtherAllergyCatalogRowReadModel(
                Name: "Insect venom",
                DescriptionFromDb: "User declared insect venom allergy.",
                DisplayLine: "Insect venom allergy declared."));
        }

        if (summary?.LatexAllergyStatus == AllergyAnswerState.Yes)
        {
            databaseRows.Add(new OtherAllergyCatalogRowReadModel(
                Name: "Latex",
                DescriptionFromDb: "User declared latex allergy.",
                DisplayLine: "Latex allergy declared."));
        }

        var customRows = customAllergies
            .Select(allergy => new CustomOtherAllergyRowReadModel(
                Name: allergy.AllergyName,
                UserNote: allergy.Note,
                DisplayLine: $"{allergy.AllergyName} | {allergy.Note}"))
            .ToList();

        return new AllergyOtherSubgroupReadModel(
            FromDatabase: DrugInteractionToolReportMapping.BuildSubsection(
                "allergies.other.from_database",
                DrugInteractionToolHeadlines.OtherAllergiesDatabase,
                databaseRows),
            Custom: DrugInteractionToolReportMapping.BuildSubsection(
                "allergies.other.custom",
                DrugInteractionToolHeadlines.OtherAllergiesCustom,
                customRows));
    }
}
