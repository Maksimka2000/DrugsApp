using DrugsApp.Domain.RegularSupplementsFeature.Entities;
using DrugsApp.Domain.RegularSupplementsFeature.Enums;

namespace DrugsApp.Features.DrugInteractionToolFeature.Shared;

/// <summary>
/// Own (custom) supplement that contains a catalog supplement the user takes through it [description].
/// </summary>
/// <param name="OwnSupplementId">Identifier of the own supplement.</param>
/// <param name="OwnSupplementName">Display name of the own supplement.</param>
internal sealed record OwnSupplementSource(
    Guid OwnSupplementId,
    string OwnSupplementName);

/// <summary>
/// Pure builder that expands the user's Active own supplements into their catalog member supplements, so interaction
/// slicing can treat every member as taken now and still name the own supplement it came from [description].
/// </summary>
internal static class OwnSupplementMembershipBuilder
{
    /// <summary>
    /// Maps every catalog supplement inside an Active own-source regular supplement to the own supplements containing it.
    /// </summary>
    /// <param name="regularSupplements">
    /// Active regular supplement rows loaded with their own supplement and its items; catalog-source rows are ignored.
    /// </param>
    /// <returns>
    /// Own supplement sources keyed by member supplement identifier, each list ordered by own supplement name and
    /// identifier; one own supplement appears at most once per member even when several rows reference it.
    /// </returns>
    internal static Dictionary<Guid, IReadOnlyList<OwnSupplementSource>> BuildSourcesBySupplementId(
        IEnumerable<ClientRegularSupplement> regularSupplements)
    {
        Dictionary<Guid, List<OwnSupplementSource>> sourcesBySupplementId = new();
        HashSet<Guid> seenOwnSupplementIds = new();

        foreach (ClientRegularSupplement regularSupplement in regularSupplements)
        {
            ClientOwnSupplement? ownSupplement = regularSupplement.OwnSupplement;
            bool isOwnSourceRow = regularSupplement.Source == RegularSupplementSource.Own && ownSupplement is not null;

            if (!isOwnSourceRow || !seenOwnSupplementIds.Add(ownSupplement!.Id))
            {
                continue;
            }

            OwnSupplementSource source = new(ownSupplement.Id, ownSupplement.Name);

            foreach (Guid memberSupplementId in ownSupplement.Items.Select(item => item.SupplementId).Distinct())
            {
                if (!sourcesBySupplementId.TryGetValue(memberSupplementId, out List<OwnSupplementSource>? sources))
                {
                    sources = new List<OwnSupplementSource>();
                    sourcesBySupplementId[memberSupplementId] = sources;
                }

                sources.Add(source);
            }
        }

        return sourcesBySupplementId.ToDictionary(
            pair => pair.Key,
            pair => (IReadOnlyList<OwnSupplementSource>)pair.Value
                .OrderBy(source => source.OwnSupplementName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(source => source.OwnSupplementId)
                .ToList());
    }
}
