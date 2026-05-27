using System;
using DrugsApp.Domain.Common;
using DrugsApp.Domain.InteractionFeature.Enums;

namespace DrugsApp.Domain.InteractionFeature.Entities;

/// <summary>
/// Read-side projection rows for atomic interaction pairs.
/// </summary>
public sealed class InteractionExpanded : BaseEntity
{
    private InteractionExpanded() : base()
    {
    }

    /// <summary>
    /// Represents the unique identifier for an interaction.
    /// This property is used to associate and maintain the relationship
    /// between an interaction and its expanded rows or other related entities.
    /// </summary>
    public Guid InteractionId { get; private set; }

    /// <summary>
    /// Represents the core interaction entity within the system.
    /// This entity captures the details and relationships of an interaction,
    /// including its severity, associated parties, and any expanded interaction rows.
    /// </summary>
    public Interaction Interaction { get; private set; } = null!;

    /// <summary>
    /// Represents the type classification of the first party involved in the interaction.
    /// This property distinguishes the nature of the entity (e.g., substance, food, or supplement),
    /// playing a key role in defining the interaction dynamics.
    /// </summary>
    public ShortInteractionType PartyTypeA { get; private set; }

    /// <summary>
    /// Represents the unique identifier for Party A in the interaction.
    /// This property is used to establish and maintain the association
    /// between Party A and its role in the interaction.
    /// </summary>
    public Guid PartyIdA { get; private set; }

    /// <summary>
    /// Represents the type of the second party involved in the interaction.
    /// This property allows the classification of the second party, such as
    /// whether it is a substance, food, or supplement, aiding in proper categorization
    /// and processing of interaction data.
    /// </summary>
    public ShortInteractionType PartyTypeB { get; private set; }

    /// <summary>
    /// Represents the unique identifier for the second party involved in an interaction.
    /// This property is used to establish and maintain the relationship
    /// between the interaction and the corresponding entity associated with Party B.
    /// </summary>
    public Guid PartyIdB { get; private set; }

    /// <summary>
    /// Creates an expanded row for an interaction.
    /// </summary>
    public static InteractionExpanded Create(
        Guid interactionId,
        ShortInteractionType partyTypeA,
        Guid partyIdA,
        ShortInteractionType partyTypeB,
        Guid partyIdB)
    {
        return new InteractionExpanded
        {
            InteractionId = interactionId,
            PartyTypeA = partyTypeA,
            PartyIdA = partyIdA,
            PartyTypeB = partyTypeB,
            PartyIdB = partyIdB
        };
    }
}
