using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.InteractionFeature.Entities;

namespace DrugsApp.Features.DrugInteractionToolFeature.Shared;

/// <summary>
/// Ordered pair of drug form identifiers used to address the interaction rows between two selected drugs [description].
/// </summary>
/// <param name="LeftFormId">Drug form whose substances are reported as the current side.</param>
/// <param name="RightFormId">Drug form whose substances are reported as the compared side.</param>
internal sealed record CrossDrugPairKey(Guid LeftFormId, Guid RightFormId);

/// <summary>
/// Builds the drug-to-drug interaction rows for every pair of selected drugs from already-loaded data [description].
/// </summary>
internal static class DrugPairInteractionBuilder
{
    /// <summary>
    /// Builds forward and reverse interaction rows for every pair of selected drugs without any database access.
    /// </summary>
    /// <param name="intakes">Selected drug forms with their active substances loaded, in report order.</param>
    /// <param name="drugDrugRows">Expanded substance-to-substance rows returned by the bulk query.</param>
    /// <param name="interactionById">Parent interactions with severity keyed by identifier.</param>
    /// <param name="substanceNameById">Active substance names keyed by identifier.</param>
    /// <returns>
    /// Rows keyed by ordered drug form pair; pairs without interactions and pairs of forms that belong to the same
    /// drug are absent.
    /// </returns>
    internal static Dictionary<CrossDrugPairKey, List<SubstancePairInteractionRowReadModel>> BuildCrossDrugPairs(
        IReadOnlyList<DrugFormIntake> intakes,
        IReadOnlyCollection<InteractionExpanded> drugDrugRows,
        IReadOnlyDictionary<Guid, Interaction> interactionById,
        IReadOnlyDictionary<Guid, string> substanceNameById)
    {
        Dictionary<CrossDrugPairKey, List<SubstancePairInteractionRowReadModel>> result = new();

        if (intakes.Count < 2 || drugDrugRows.Count == 0)
        {
            return result;
        }

        ILookup<(Guid First, Guid Second), InteractionExpanded> rowsBySubstancePair = drugDrugRows
            .ToLookup(row => BuildSubstancePairKey(row.PartyIdA, row.PartyIdB));

        HashSet<Guid>[] substanceIdsByIntake = intakes
            .Select(intake => intake.Drug.ActiveSubstances.Select(substance => substance.Id).ToHashSet())
            .ToArray();

        for (int leftIndex = 0; leftIndex < intakes.Count; leftIndex++)
        {
            for (int rightIndex = leftIndex + 1; rightIndex < intakes.Count; rightIndex++)
            {
                DrugFormIntake left = intakes[leftIndex];
                DrugFormIntake right = intakes[rightIndex];

                if (left.Drug.Id == right.Drug.Id)
                {
                    continue;
                }

                HashSet<Guid> leftIds = substanceIdsByIntake[leftIndex];
                HashSet<Guid> rightIds = substanceIdsByIntake[rightIndex];

                List<InteractionExpanded> pairRows = CollectPairRows(leftIds, rightIds, rowsBySubstancePair);
                if (pairRows.Count == 0)
                {
                    continue;
                }

                bool bothOral = left.Form.IsOral && right.Form.IsOral;

                List<SubstancePairInteractionRowReadModel> forwardRows = MapDrugDrugPairRows(
                    pairRows,
                    leftIds,
                    rightIds,
                    bothOral,
                    interactionById,
                    substanceNameById);

                List<SubstancePairInteractionRowReadModel> reverseRows = forwardRows
                    .Select(row => row with
                    {
                        CurrentActiveSubstanceId = row.ComparedActiveSubstanceId,
                        CurrentActiveSubstanceName = row.ComparedActiveSubstanceName,
                        ComparedActiveSubstanceId = row.CurrentActiveSubstanceId,
                        ComparedActiveSubstanceName = row.CurrentActiveSubstanceName,
                        DisplayLine = DrugInteractionToolReportMapping.BuildSubstancePairDisplayLine(
                            row.ComparedActiveSubstanceName,
                            row.CurrentActiveSubstanceName,
                            row.DefaultText,
                            row.StomachText,
                            row.Severity)
                    })
                    .ToList();

                result[new CrossDrugPairKey(left.Form.Id, right.Form.Id)] = forwardRows;
                result[new CrossDrugPairKey(right.Form.Id, left.Form.Id)] = reverseRows;
            }
        }

        return result;
    }

    private static (Guid First, Guid Second) BuildSubstancePairKey(Guid substanceId, Guid otherSubstanceId) =>
        substanceId.CompareTo(otherSubstanceId) <= 0
            ? (substanceId, otherSubstanceId)
            : (otherSubstanceId, substanceId);

    private static List<InteractionExpanded> CollectPairRows(
        HashSet<Guid> leftIds,
        HashSet<Guid> rightIds,
        ILookup<(Guid First, Guid Second), InteractionExpanded> rowsBySubstancePair)
    {
        List<InteractionExpanded> pairRows = new();
        HashSet<Guid> collectedRowIds = new();

        foreach (Guid leftId in leftIds)
        {
            foreach (Guid rightId in rightIds)
            {
                if (leftId == rightId)
                {
                    continue;
                }

                foreach (InteractionExpanded row in rowsBySubstancePair[BuildSubstancePairKey(leftId, rightId)])
                {
                    if (collectedRowIds.Add(row.Id))
                    {
                        pairRows.Add(row);
                    }
                }
            }
        }

        return pairRows;
    }

    private static List<SubstancePairInteractionRowReadModel> MapDrugDrugPairRows(
        IReadOnlyCollection<InteractionExpanded> expandedRows,
        HashSet<Guid> currentSubstanceIds,
        HashSet<Guid> comparedSubstanceIds,
        bool includeStomachText,
        IReadOnlyDictionary<Guid, Interaction> interactionById,
        IReadOnlyDictionary<Guid, string> substanceNameById)
    {
        List<SubstancePairInteractionRowReadModel> rows = new();

        foreach (InteractionExpanded expanded in expandedRows.OrderBy(row => row.PartyIdA).ThenBy(row => row.PartyIdB))
        {
            if (!interactionById.TryGetValue(expanded.InteractionId, out Interaction? interaction))
            {
                continue;
            }

            Guid currentId;
            Guid comparedId;

            if (currentSubstanceIds.Contains(expanded.PartyIdA) && comparedSubstanceIds.Contains(expanded.PartyIdB))
            {
                currentId = expanded.PartyIdA;
                comparedId = expanded.PartyIdB;
            }
            else if (currentSubstanceIds.Contains(expanded.PartyIdB) && comparedSubstanceIds.Contains(expanded.PartyIdA))
            {
                currentId = expanded.PartyIdB;
                comparedId = expanded.PartyIdA;
            }
            else
            {
                continue;
            }

            string currentName = substanceNameById[currentId];
            string comparedName = substanceNameById[comparedId];
            string severity = interaction.InteractionSeverity.Name;
            string? stomachText = includeStomachText ? interaction.StomachText : null;
            string defaultText = interaction.DefaultText ?? string.Empty;

            rows.Add(new SubstancePairInteractionRowReadModel(
                CurrentActiveSubstanceId: currentId,
                CurrentActiveSubstanceName: currentName,
                ComparedActiveSubstanceId: comparedId,
                ComparedActiveSubstanceName: comparedName,
                InteractionId: interaction.Id,
                DefaultText: defaultText,
                StomachText: stomachText,
                Severity: severity,
                DisplayLine: DrugInteractionToolReportMapping.BuildSubstancePairDisplayLine(
                    currentName,
                    comparedName,
                    defaultText,
                    stomachText,
                    severity)));
        }

        return rows
            .OrderBy(row => row.CurrentActiveSubstanceName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.ComparedActiveSubstanceName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.InteractionId)
            .ToList();
    }
}
