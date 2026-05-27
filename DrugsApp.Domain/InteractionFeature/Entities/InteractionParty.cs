using System;
using DrugsApp.Domain.Common;
using DrugsApp.Domain.InteractionFeature.Enums;
using ErrorOr;

namespace DrugsApp.Domain.InteractionFeature.Entities;

/// <summary>
/// Represents a single party participating in an interaction.
/// </summary>
public sealed class InteractionParty : BaseEntity
{
    private InteractionParty() : base()
    {
    }

    /// <summary>
    /// Gets the identifier of the interaction to which the party belongs.
    /// </summary>
    /// <remarks>
    /// This property serves as a foreign key linking an interaction party to its associated interaction.
    /// It ensures data integrity and facilitates the association of parties with specific interaction records.
    /// </remarks>
    public Guid InteractionId { get; private set; }

    /// <summary>
    /// Represents the interaction associated with an <see cref="InteractionParty"/> entity.
    /// </summary>
    /// <remarks>
    /// This property establishes a relationship between the <see cref="InteractionParty"/> and the <see cref="Interaction"/> entities,
    /// signifying the specific interaction relevant to the interaction party.
    /// </remarks>
    public Interaction Interaction { get; private set; } = null!;

    /// <summary>
    /// Gets the type of the interaction party.
    /// </summary>
    /// <remarks>
    /// This property specifies the classification of a party involved in an interaction,
    /// such as a substance, food, supplement, or their respective groups.
    /// It determines the context in which the party participates in the interaction and
    /// facilitates appropriate data organization and retrieval.
    /// </remarks>
    public InteractionType PartyType { get; private set; }

    /// <summary>
    /// Gets the identifier of the active substance associated with the interaction party.
    /// </summary>
    /// <remarks>
    /// This property identifies the specific active substance connected to the party within an interaction context.
    /// It is utilized when the party type corresponds to an individual active substance.
    /// </remarks>
    public Guid? ActiveSubstanceId { get; private set; }

    /// <summary>
    /// Represents the identifier for a group of active substances associated with an interaction party.
    /// </summary>
    /// <remarks>
    /// This property is used to establish a relationship between an interaction and a group of active substances.
    /// It is primarily relevant for interactions categorized under the <c>SubstanceGroup</c> type, enabling the
    /// resolution and association of all atomic substances within the specified group for interaction processing.
    /// </remarks>
    public Guid? ActiveSubstanceGroupId { get; private set; }

    /// <summary>
    /// Gets the identifier of the food entity associated with the interaction party.
    /// </summary>
    /// <remarks>
    /// This property is used to establish a reference between an interaction party and a specific food item.
    /// It serves as a foreign key and ensures proper linkage and data consistency within the interaction context.
    /// </remarks>
    public Guid? FoodId { get; private set; }

    /// <summary>
    /// Gets the identifier of the associated food group in the interaction.
    /// </summary>
    /// <remarks>
    /// This property represents a foreign key that links an interaction party to a specific food group.
    /// It enables the categorization of foods within a group and supports scenarios where interactions
    /// are defined at the food group level, ensuring proper association and data consistency.
    /// </remarks>
    public Guid? FoodGroupId { get; private set; }

    /// <summary>
    /// Gets the identifier of the supplement associated with the interaction party.
    /// </summary>
    /// <remarks>
    /// This property is used to link an interaction party to a specific supplement. It serves
    /// as a key for identifying the supplement in scenarios where interaction data involves supplements.
    /// </remarks>
    public Guid? SupplementId { get; private set; }

    /// <summary>
    /// Gets the identifier of the supplement group associated with the interaction party.
    /// </summary>
    /// <remarks>
    /// This property is used when the interaction party represents a group of supplements.
    /// It allows the interaction party to link to a specific supplement group, ensuring proper
    /// categorization and enabling operations that involve grouped supplements.
    /// </remarks>
    public Guid? SupplementGroupId { get; private set; }

    /// <summary>
    /// Creates a party for the specified interaction and type.
    /// </summary>
    /// <param name="interactionId">Owning interaction identifier.</param>
    /// <param name="partyType">Interaction party type.</param>
    /// <param name="partyId">Identifier of the referenced entity.</param>
    /// <returns>New party or validation error.</returns>
    public static ErrorOr<InteractionParty> Create(
        Guid interactionId,
        InteractionType partyType,
        Guid partyId)
    {
        if (interactionId == Guid.Empty)
        {
            return Error.Validation(description: "InteractionId must not be empty.");
        }

        if (partyId == Guid.Empty)
        {
            return Error.Validation(description: "PartyId must not be empty.");
        }

        var party = new InteractionParty
        {
            InteractionId = interactionId,
            PartyType = partyType
        };

        switch (partyType)
        {
            case InteractionType.Substance:
                party.ActiveSubstanceId = partyId;
                break;
            case InteractionType.SubstanceGroup:
                party.ActiveSubstanceGroupId = partyId;
                break;
            case InteractionType.Food:
                party.FoodId = partyId;
                break;
            case InteractionType.FoodGroup:
                party.FoodGroupId = partyId;
                break;
            case InteractionType.Supplement:
                party.SupplementId = partyId;
                break;
            case InteractionType.SupplementGroup:
                party.SupplementGroupId = partyId;
                break;
            default:
                return Error.Validation(description: "Unsupported interaction party type.");
        }

        return party;
    }
}
