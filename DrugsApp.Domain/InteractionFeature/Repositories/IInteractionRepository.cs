using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.InteractionFeature.Entities;
using DrugsApp.Domain.InteractionFeature.Enums;

namespace DrugsApp.Domain.InteractionFeature.Repositories;

/// <summary>
/// Persistence contract for interaction aggregate and projections.
/// </summary>
public interface IInteractionRepository
{
    Task<List<Interaction>> SearchByPartiesAsync(
        InteractionKind interactionKind,
        InteractionPartyKey? leftPartyKey,
        InteractionPartyKey? rightPartyKey,
        InteractionSortBy sortBy,
        bool ascending,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<int> CountByPartiesAsync(
        InteractionKind interactionKind,
        InteractionPartyKey? leftPartyKey,
        InteractionPartyKey? rightPartyKey,
        CancellationToken cancellationToken);

    Task<Interaction?> GetByIdAsync(Guid interactionId, CancellationToken cancellationToken);

    Task<InteractionSeverity?> GetInteractionSeverityByIdAsync(Guid? interactionSeverityId,
        CancellationToken cancellationToken);

    Task<Interaction?> GetByCanonicalPairAsync(
        InteractionPartyKey firstParty,
        InteractionPartyKey secondParty,
        CancellationToken cancellationToken);

    Task AddAsync(Interaction interaction, CancellationToken cancellationToken);

    void Remove(Interaction interaction);

    Task<bool> AnyPartyReferencesGroupAsync(
        InteractionType groupPartyType,
        Guid groupId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Guid>> GetInteractionIdsByGroupPartyAsync(
        InteractionType groupPartyType,
        Guid groupId,
        CancellationToken cancellationToken);

    Task<Dictionary<Guid, IReadOnlyCollection<InteractionParty>>> GetPartiesByInteractionIdsAsync(
        IEnumerable<Guid> interactionIds,
        CancellationToken cancellationToken);

    Task DeleteExpandedRowsByAtomicPairsAsync(
        Guid interactionId,
        IEnumerable<InteractionAtomicPair> atomicPairs,
        CancellationToken cancellationToken);

    Task InsertExpandedRowsIgnoreConflictsAsync(
        Guid interactionId,
        IEnumerable<InteractionAtomicPair> atomicPairs,
        CancellationToken cancellationToken);

    /// <summary>
    /// Returns expanded projection rows matching atomic party id sets (pair discovery only).
    /// </summary>
    Task<List<InteractionExpanded>> SearchExpandedRowsByAtomicIdsAsync(
        ShortInteractionType typeA,
        IReadOnlyCollection<Guid> idsA,
        ShortInteractionType typeB,
        IReadOnlyCollection<Guid> idsB,
        CancellationToken cancellationToken);

    /// <summary>
    /// Bulk loads parent interactions with severity and parties for report mapping.
    /// </summary>
    Task<List<Interaction>> GetByIdsAsync(
        IReadOnlyCollection<Guid> interactionIds,
        CancellationToken cancellationToken);

    /// <summary>
    /// Returns expanded atomic pairs for the given left active substances joined to parent interaction texts.
    /// </summary>
    Task<List<InteractionExpandedProjection>> ListExpandedByLeftActiveSubstanceIdsAsync(
        InteractionKind interactionKind,
        IReadOnlyCollection<Guid> leftActiveSubstanceIds,
        CancellationToken cancellationToken);

    /// <summary>
    /// Lists interactions where the active substance participates as an atomic party.
    /// </summary>
    Task<List<Interaction>> ListByActiveSubstanceIdAsync(
        Guid activeSubstanceId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Lists interactions where the supplement participates as an atomic party.
    /// </summary>
    Task<List<Interaction>> ListBySupplementIdAsync(
        Guid supplementId,
        CancellationToken cancellationToken);
}

/// <summary>
/// Party key combining type and identifier.
/// </summary>
/// <param name="PartyType">Party type.</param>
/// <param name="PartyId">Party identifier.</param>
public sealed record InteractionPartyKey(InteractionType PartyType, Guid PartyId);

/// <summary>
/// Atomic pair used for expanded projection operations.
/// </summary>
/// <param name="PartyTypeA">First atomic party type.</param>
/// <param name="PartyIdA">First atomic party id.</param>
/// <param name="PartyTypeB">Second atomic party type.</param>
/// <param name="PartyIdB">Second atomic party id.</param>
public sealed record InteractionAtomicPair(
    ShortInteractionType PartyTypeA,
    Guid PartyIdA,
    ShortInteractionType PartyTypeB,
    Guid PartyIdB);

/// <summary>
/// Read-side projection of an expanded interaction pair with parent interaction texts.
/// </summary>
/// <param name="ExpandedRowId">Expanded row identifier.</param>
/// <param name="InteractionId">Parent interaction identifier.</param>
/// <param name="LeftPartyType">Left atomic party type.</param>
/// <param name="LeftPartyId">Left atomic party identifier.</param>
/// <param name="RightPartyType">Right atomic party type.</param>
/// <param name="RightPartyId">Right atomic party identifier.</param>
/// <param name="DefaultText">Default interaction text from parent interaction.</param>
/// <param name="StomachText">Stomach interaction text from parent interaction.</param>
/// <param name="SeverityName">Severity label from parent interaction.</param>
/// <param name="SeverityNumber">Severity rank used for ordering.</param>
public sealed record InteractionExpandedProjection(
    Guid ExpandedRowId,
    Guid InteractionId,
    ShortInteractionType LeftPartyType,
    Guid LeftPartyId,
    ShortInteractionType RightPartyType,
    Guid RightPartyId,
    string? DefaultText,
    string? StomachText,
    string SeverityName,
    short SeverityNumber);
