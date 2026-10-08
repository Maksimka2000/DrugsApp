using DrugsApp.Domain.InteractionFeature.Enums;
using DrugsApp.Domain.InteractionFeature.Repositories;

namespace DrugsApp.Features.InteractionFeature.ApplicationServices;

/// <summary>
/// Builds canonical, de-duplicated atomic pairs for the interaction expanded projection [description].
/// </summary>
internal static class InteractionAtomicPairs
{
    /// <summary>
    /// Builds every canonical pair between the left and right atomic ids, keeping each pair once.
    /// </summary>
    /// <param name="leftType">Atomic type of the left ids.</param>
    /// <param name="leftIds">Left atomic ids.</param>
    /// <param name="rightType">Atomic type of the right ids.</param>
    /// <param name="rightIds">Right atomic ids.</param>
    /// <returns>Distinct canonical pairs in first-seen order.</returns>
    internal static List<InteractionAtomicPair> BuildPairs(
        ShortInteractionType leftType,
        IReadOnlyCollection<Guid> leftIds,
        ShortInteractionType rightType,
        IReadOnlyCollection<Guid> rightIds)
    {
        var pairs = new List<InteractionAtomicPair>();
        var seenPairs = new HashSet<InteractionAtomicPair>();

        foreach (Guid leftId in leftIds)
        {
            foreach (Guid rightId in rightIds)
            {
                if (leftType == rightType && leftId == rightId)
                {
                    continue;
                }

                InteractionAtomicPair pair = CanonicalizePair(leftType, leftId, rightType, rightId);
                if (seenPairs.Add(pair))
                {
                    pairs.Add(pair);
                }
            }
        }

        return pairs;
    }

    /// <summary>
    /// Orders the two atomic parties so the same pair always has one representation.
    /// </summary>
    /// <param name="typeA">First atomic type.</param>
    /// <param name="idA">First atomic id.</param>
    /// <param name="typeB">Second atomic type.</param>
    /// <param name="idB">Second atomic id.</param>
    /// <returns>The pair with the smaller type, then smaller id, first.</returns>
    internal static InteractionAtomicPair CanonicalizePair(
        ShortInteractionType typeA,
        Guid idA,
        ShortInteractionType typeB,
        Guid idB)
    {
        bool keepOrder = typeA < typeB || typeA == typeB && idA.CompareTo(idB) <= 0;
        return keepOrder
            ? new InteractionAtomicPair(typeA, idA, typeB, idB)
            : new InteractionAtomicPair(typeB, idB, typeA, idA);
    }
}
