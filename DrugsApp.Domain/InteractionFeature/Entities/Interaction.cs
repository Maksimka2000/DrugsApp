using System;
using System.Collections.Generic;
using System.Linq;
using DrugsApp.Domain.Common;
using DrugsApp.Domain.InteractionFeature.Enums;
using ErrorOr;

namespace DrugsApp.Domain.InteractionFeature.Entities;

/// <summary>
/// Canonical interaction aggregate root.
/// </summary>
public sealed class Interaction : BaseEntity
{
    private Interaction() : base() {} // for the EF Core

    // Attributes 
    /// <summary>
    /// Represents the default descriptive text associated with the interaction.
    /// This property may be null or empty, and its value can be modified via
    /// specified domain logic to ensure consistency and validation.
    /// </summary>
    public string? DefaultText { get; private set; }

    /// <summary>
    /// Represents the supplementary descriptive text related to the interaction,
    /// often used to detail specific contextual information.
    /// This property can be null or empty, and its value may be updated
    /// through controlled domain logic to ensure accuracy and adherence
    /// to business rules.
    /// </summary>
    public string? StomachText { get; private set; }

    // Relationships
    /// <summary>
    /// Represents the unique identifier associated with the severity level of an interaction.
    /// This property establishes a relationship with the <c>InteractionSeverity</c> entity
    /// and determines the corresponding severity classification within the domain.
    /// </summary>
    public Guid InteractionSeverityId { get; private set; } // FK to InteractionSeverity

    /// <summary>
    /// Represents the severity level associated with a specific interaction.
    /// This property is a required foreign key reference to the InteractionSeverity entity,
    /// encapsulating the importance or impact classification of the interaction.
    /// </summary>
    public InteractionSeverity InteractionSeverity { get; private set; } = null!; // One-to-One relationship with InteractionSeverity

    private readonly List<InteractionParty> _parties = new();
    /// <summary>
    /// Provides a read-only collection of parties associated with the interaction.
    /// Each party represents an entity linked to the interaction, defining its role
    /// within the interaction context. The collection is managed internally and
    /// modifications are handled through predefined domain logic.
    /// </summary>
    public IReadOnlyCollection<InteractionParty> Parties => _parties; // One-to-Many relationship with InteractionParty, can have only 2 parties

    private readonly List<InteractionExpanded> _expandedRows = new();
    /// <summary>
    /// Provides access to the collection of expanded rows associated with the interaction.
    /// This property exposes the related <see cref="InteractionExpanded"/> entities as a read-only collection.
    /// </summary>
    public IReadOnlyCollection<InteractionExpanded> ExpandedRows => _expandedRows; // One-to-Many relationship with InteractionExpanded, can have only 2 parties

    /// <summary>
    /// Creates a new interaction with required severity and description texts.
    /// </summary>
    public static ErrorOr<Interaction> Create(
        Guid interactionSeverityId,
        string? defaultText,
        string? stomachText)
    {
        Interaction interaction = new()
        {
            InteractionSeverityId = interactionSeverityId
        };

        ErrorOr<Success> validationResult = interaction.UpdateTexts(
            defaultText,
            stomachText,
            clearDefaultText: false,
            clearStomachText: false);
        if (validationResult.IsError)
        {
            return validationResult.Errors;
        }

        return interaction;
    }

    /// <summary>
    /// Updates interaction texts while enforcing minimum text rule.
    /// </summary>
    public ErrorOr<Success> UpdateTexts(
        string? defaultText,
        string? stomachText,
        bool clearDefaultText,
        bool clearStomachText)
    {
        string? nextDefaultText = DefaultText;
        string? nextStomachText = StomachText;

        if (clearDefaultText)
        {
            nextDefaultText = null;
        }

        if (clearStomachText)
        {
            nextStomachText = null;
        }

        if (defaultText is not null)
        {
            nextDefaultText = NormalizeText(defaultText);
        }

        if (stomachText is not null)
        {
            nextStomachText = NormalizeText(stomachText);
        }

        bool hasDefaultText = !string.IsNullOrWhiteSpace(nextDefaultText);
        bool hasStomachText = !string.IsNullOrWhiteSpace(nextStomachText);

        if (!hasDefaultText && !hasStomachText)
        {
            return Error.Validation(
                description: "At least one interaction text is required.",
                metadata: new Dictionary<string, object>
                {
                    ["rule"] = "MinimumInteractionText"
                });
        }

        DefaultText = hasDefaultText ? nextDefaultText : null;
        StomachText = hasStomachText ? nextStomachText : null;

        return Result.Success;
    }

    /// <summary>
    /// Updates severity reference.
    /// </summary>
    public void UpdateSeverity(Guid interactionSeverityId)
    {
        if (interactionSeverityId == Guid.Empty || InteractionSeverityId == interactionSeverityId)
        {
            return;
        }

        InteractionSeverityId = interactionSeverityId;
    }

    /// <summary>
    /// Adds a party row to the aggregate if it belongs to this interaction.
    /// </summary>
    public void AddParty(InteractionParty party)
    {
        if (party.InteractionId != Id)
        {
            return;
        }

        if (_parties.Any(existing => existing.Id == party.Id))
        {
            return;
        }

        _parties.Add(party);
    }

    /// <summary>
    /// Adds an expanded projection row to the aggregate if it belongs to this interaction.
    /// </summary>
    public void AddExpandedRow(InteractionExpanded expandedRow)
    {
        if (expandedRow.InteractionId != Id)
        {
            return;
        }

        if (_expandedRows.Any(existing => existing.Id == expandedRow.Id))
        {
            return;
        }

        _expandedRows.Add(expandedRow);
    }

    /// <summary>
    /// Replaces expanded rows with the provided collection.
    /// </summary>
    public void ReplaceExpandedRows(IEnumerable<InteractionExpanded> expandedRows)
    {
        _expandedRows.Clear();
        foreach (InteractionExpanded expandedRow in expandedRows)
        {
            AddExpandedRow(expandedRow);
        }
    }

    private static string? NormalizeText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string trimmed = value.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }
}
