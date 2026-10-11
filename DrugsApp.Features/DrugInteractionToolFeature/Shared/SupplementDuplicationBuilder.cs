using DrugsApp.Domain.RegularSupplementsFeature.Entities;
using DrugsApp.Domain.RegularSupplementsFeature.Enums;
using DrugsApp.Domain.SupplementFeature.Entities;

namespace DrugsApp.Features.DrugInteractionToolFeature.Shared;

/// <summary>
/// Pure builder of supplement duplication rows: a catalog supplement reaching the user through more than one
/// source (selected for the report, taken directly and/or inside own supplements) [description].
/// </summary>
internal static class SupplementDuplicationBuilder
{
    /// <summary>
    /// Builds one row per catalog supplement that appears in two or more sources.
    /// </summary>
    /// <param name="regularSupplements">Active regular supplements with catalog supplement and own supplement members loaded.</param>
    /// <param name="selectedSupplements">Catalog supplements selected for the report; a selected supplement is one source.</param>
    /// <returns>Rows ordered by supplement name, possibly empty.</returns>
    internal static List<SupplementDuplicationRowReadModel> BuildRows(
        IReadOnlyCollection<ClientRegularSupplement> regularSupplements,
        IReadOnlyCollection<Supplement>? selectedSupplements = null)
    {
        Dictionary<Guid, Supplement> supplementById = new();
        HashSet<Guid> directSupplementIds = new();
        Dictionary<Guid, List<ClientOwnSupplement>> ownSupplementsBySupplementId = new();
        HashSet<Guid> selectedSupplementIds = new();

        foreach (Supplement selectedSupplement in selectedSupplements ?? [])
        {
            supplementById[selectedSupplement.Id] = selectedSupplement;
            selectedSupplementIds.Add(selectedSupplement.Id);
        }

        foreach (ClientRegularSupplement row in regularSupplements)
        {
            if (row.Source == RegularSupplementSource.Database && row.Supplement is not null)
            {
                supplementById[row.Supplement.Id] = row.Supplement;
                directSupplementIds.Add(row.Supplement.Id);
                continue;
            }

            if (row.Source != RegularSupplementSource.Own || row.OwnSupplement is null)
            {
                continue;
            }

            foreach (ClientOwnSupplementItem item in row.OwnSupplement.Items.Where(item => item.Supplement is not null))
            {
                supplementById.TryAdd(item.Supplement.Id, item.Supplement);

                if (!ownSupplementsBySupplementId.TryGetValue(item.Supplement.Id, out List<ClientOwnSupplement>? owners))
                {
                    owners = new List<ClientOwnSupplement>();
                    ownSupplementsBySupplementId[item.Supplement.Id] = owners;
                }

                // the same own supplement counts once even when listed twice in the regular list
                if (owners.All(owner => owner.Id != row.OwnSupplement.Id))
                {
                    owners.Add(row.OwnSupplement);
                }
            }
        }

        List<SupplementDuplicationRowReadModel> rows = new();

        foreach ((Guid supplementId, Supplement supplement) in supplementById)
        {
            bool isTakenDirectly = directSupplementIds.Contains(supplementId);
            bool isSelected = selectedSupplementIds.Contains(supplementId);
            List<ClientOwnSupplement> owners = ownSupplementsBySupplementId.GetValueOrDefault(supplementId) ?? [];
            int sourceCount = (isTakenDirectly ? 1 : 0) + (isSelected ? 1 : 0) + owners.Count;
            if (sourceCount < 2)
            {
                continue;
            }

            string summaryText = BuildSupplementSummaryText(supplement);

            List<DuplicationSupplementSummaryReadModel> affectedSupplements = new();
            if (isTakenDirectly)
            {
                affectedSupplements.Add(new DuplicationSupplementSummaryReadModel(
                    supplementId,
                    summaryText,
                    DuplicationDrugSource.TakingNow));
            }

            if (isSelected)
            {
                affectedSupplements.Add(new DuplicationSupplementSummaryReadModel(
                    supplementId,
                    summaryText,
                    DuplicationDrugSource.Selected));
            }

            IReadOnlyCollection<DuplicationOwnSupplementSummaryReadModel> affectedOwnSupplements = owners
                .OrderBy(owner => owner.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(owner => owner.Id)
                .Select(owner => new DuplicationOwnSupplementSummaryReadModel(
                    OwnSupplementId: owner.Id,
                    SupplementSummaryText: BuildOwnSupplementSummaryText(owner),
                    Source: DuplicationDrugSource.TakingNow))
                .ToList();

            rows.Add(new SupplementDuplicationRowReadModel(
                SupplementId: supplementId,
                SupplementSummaryText: summaryText,
                AffectedSupplements: affectedSupplements,
                AffectedOwnSupplements: affectedOwnSupplements,
                DisplayLine: isSelected
                    ? $"Supplement: {summaryText} IS SELECTED AND ALREADY TAKEN REGULARLY WHICH IS CONSIDERED AS A DUPLICATION."
                    : $"Supplement: {summaryText} IS TAKEN REGULARLY FROM MORE THAN ONE SOURCE WHICH IS CONSIDERED AS A DUPLICATION.",
                DuplicationKind: DuplicationKind.Supplement));
        }

        return rows
            .OrderBy(row => row.SupplementSummaryText, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.SupplementId)
            .ToList();
    }

    private static string BuildSupplementSummaryText(Supplement supplement) =>
        DrugInteractionToolReportMapping.BuildNameWithAliases(
            supplement.Name ?? string.Empty,
            supplement.Aliases.ToList());

    private static string BuildOwnSupplementSummaryText(ClientOwnSupplement ownSupplement)
    {
        List<string> memberNames = ownSupplement.Items
            .Where(item => item.Supplement is not null)
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.SupplementId)
            .Select(item => item.Supplement.Name ?? string.Empty)
            .ToList();

        return memberNames.Count == 0
            ? ownSupplement.Name
            : $"{ownSupplement.Name} | Contains: {string.Join(", ", memberNames)}";
    }
}
