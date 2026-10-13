using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.AllergiesFeature.Entities;
using DrugsApp.Domain.AllergiesFeature.Enums;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.DrugProblemsFeature.Entities;
using DrugsApp.Domain.DrugProblemsFeature.Enums;
using DrugsApp.Features.AllergiesFeature.Shared;

namespace DrugsApp.Features.DrugInteractionToolFeature.Shared;

/// <summary>
/// Precomputed per-drug-form and per-substance lookups so the per-drug loop performs zero linear scans.
/// </summary>
internal sealed record AllergyIndexes(
    Dictionary<Guid, List<SpecificDrugProblemRowReadModel>> ProblemsByDrugFormId,
    Dictionary<Guid, SpecificDrugDirectAllergyRowReadModel> DirectAllergyByDrugFormId,
    Dictionary<Guid, List<ActiveSubstanceAllergyOverlapRowReadModel>> OverlapBySubstanceId);

/// <summary>
/// Builds the per-drug-form problem and allergy lookups and slices them for each selected drug [description].
/// </summary>
internal static class DrugAllergyProblemIndexBuilder
{
    private static readonly List<ActiveSubstanceAllergyOverlapRowReadModel> EmptyOverlapRows = new();

    internal static AllergyIndexes BuildAllergyIndexes(
        List<ClientDrugAllergy> drugAllergies,
        List<DrugProblem> drugProblems)
    {
        var problemsByDrugFormId = drugProblems
            .Where(problem => problem.Source == DrugProblemSource.Database && problem.DrugFormId.HasValue)
            .GroupBy(problem => problem.DrugFormId!.Value)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(problem => new SpecificDrugProblemRowReadModel(
                        ProblemDescription: problem.ProblemNote,
                        HeadlineText:
                        $"THIS DRUG WAS CHOSEN BY THE USER AS PROBLEMATIC AND HAS THIS DESCRIPTION OF A PROBLEM {problem.ProblemNote}"))
                    .ToList());

        var directAllergyByDrugFormId = drugAllergies
            .Where(allergy => allergy.Source == AllergyRecordSource.Database && allergy.DrugFormId.HasValue)
            .GroupBy(allergy => allergy.DrugFormId!.Value)
            .ToDictionary(
                group => group.Key,
                group => new SpecificDrugDirectAllergyRowReadModel(
                    ProblemDescription: group.First().Note ?? string.Empty,
                    HeadlineText:
                    $"THIS DRUG WAS MARKED BY THE USER AS ALLERGIC: {group.First().Note ?? string.Empty}"));

        Dictionary<Guid, List<ActiveSubstanceAllergyOverlapRowReadModel>> overlapBySubstanceId = new();

        foreach (ClientDrugAllergy allergy in drugAllergies.Where(allergy =>
                     allergy.Source == AllergyRecordSource.Database && allergy.DrugForm?.Drug is not null))
        {
            Drug sourceDrug = allergy.DrugForm!.Drug;
            string allergyDisplay = allergy.MapToDto().DisplayName;

            foreach (ActiveSubstance substance in sourceDrug.ActiveSubstances)
            {
                if (!overlapBySubstanceId.TryGetValue(substance.Id, out List<ActiveSubstanceAllergyOverlapRowReadModel>? list))
                {
                    list = new List<ActiveSubstanceAllergyOverlapRowReadModel>();
                    overlapBySubstanceId[substance.Id] = list;
                }

                list.Add(new ActiveSubstanceAllergyOverlapRowReadModel(
                    ActiveSubstanceId: substance.Id,
                    ActiveSubstanceName: substance.ActiveSubstanceName,
                    SourceAllergyDrugDisplay: allergyDisplay,
                    UserNote: allergy.Note,
                    HeadlineText:
                    $"THIS ACTIVE SUBSTANCE WAS CHOSEN AS THE ALLERGIC BY THE USER IN ANOTHER DRUG: {allergyDisplay}",
                    DisplayLine:
                    $"{substance.ActiveSubstanceName} <= allergic via {allergyDisplay} | {allergy.Note}"));
            }
        }

        var dedupedOverlapBySubstanceId = overlapBySubstanceId.ToDictionary(
            pair => pair.Key,
            pair => pair.Value
                .GroupBy(row => row.SourceAllergyDrugDisplay)
                .Select(group => group.First())
                .ToList());

        return new AllergyIndexes(problemsByDrugFormId, directAllergyByDrugFormId, dedupedOverlapBySubstanceId);
    }

    internal static SpecificDrugProblemsSectionReadModel? SliceProblems(Guid drugFormId, AllergyIndexes indexes)
    {
        if (!indexes.ProblemsByDrugFormId.TryGetValue(drugFormId, out List<SpecificDrugProblemRowReadModel>? rows)
            || rows.Count == 0)
        {
            return null;
        }

        return new SpecificDrugProblemsSectionReadModel(
            SectionCode: "specific_drug.problems",
            RecordCount: rows.Count,
            Rows: rows);
    }

    internal static SpecificDrugAllergiesSectionReadModel? SliceAllergies(
        Guid drugFormId,
        Drug drug,
        AllergyIndexes indexes)
    {
        SpecificDrugDirectAllergyRowReadModel? direct = indexes.DirectAllergyByDrugFormId
            .GetValueOrDefault(drugFormId);

        var overlapRows = drug.ActiveSubstances
            .SelectMany(substance => indexes.OverlapBySubstanceId.GetValueOrDefault(substance.Id, EmptyOverlapRows))
            .OrderBy(row => row.ActiveSubstanceName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (direct is null && overlapRows.Count == 0)
        {
            return null;
        }

        return new SpecificDrugAllergiesSectionReadModel(
            SectionCode: "specific_drug.allergies",
            RecordCount: overlapRows.Count + (direct is null ? 0 : 1),
            DirectDrugAllergy: direct,
            ActiveSubstanceOverlapRows: overlapRows);
    }
}
