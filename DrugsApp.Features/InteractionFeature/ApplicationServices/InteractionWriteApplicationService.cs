using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.InteractionFeature.Entities;
using DrugsApp.Domain.InteractionFeature.Enums;
using DrugsApp.Domain.InteractionFeature.Repositories;
using ErrorOr;

namespace DrugsApp.Features.InteractionFeature.ApplicationServices;


/// <summary>
/// Input describing all data required to create an interaction. DTO which accepts data from the handler
/// </summary> 
/// <param name="InteractionSeverityId">Severity identifier.</param>
/// <param name="DefaultText">Default description text.</param>
/// <param name="StomachText">Stomach-specific description text.</param>
/// <param name="LeftParty">Left party canonical key.</param>
/// <param name="RightParty">Right party canonical key.</param>
/// <param name="LeftAtomicType">Atomic type for left side (expanded rows).</param>
/// <param name="LeftAtomicIds">Atomic ids for left side.</param>
/// <param name="RightAtomicType">Atomic type for right side (expanded rows).</param>
/// <param name="RightAtomicIds">Atomic ids for right side.</param>
public sealed record CreateInteractionWorkflowInput(
    Guid InteractionSeverityId,
    string? DefaultText,
    string? StomachText,
    InteractionPartyKey LeftParty,
    InteractionPartyKey RightParty,
    ShortInteractionType LeftAtomicType,
    IReadOnlyCollection<Guid> LeftAtomicIds,
    ShortInteractionType RightAtomicType,
    IReadOnlyCollection<Guid> RightAtomicIds);

/// <summary>
/// Output of the create interaction workflow.
/// </summary>
/// <param name="InteractionId">Interaction identifier.</param>
/// <param name="DefaultText">Default description text.</param>
/// <param name="StomachText">Stomach-specific description text.</param>
public sealed record CreateInteractionWorkflowOutput(
    Guid InteractionId,
    string? DefaultText,
    string? StomachText);

internal sealed class InteractionWriteApplicationService(IInteractionRepository _interactionRepository)
    : IInteractionWriteApplicationService
{
    public async Task<ErrorOr<CreateInteractionWorkflowOutput>> CreateAsync(
        CreateInteractionWorkflowInput input,
        CancellationToken cancellationToken)
    {
        // create the interaction
        ErrorOr<Interaction> interactionResult = Interaction.Create(
            input.InteractionSeverityId,
            input.DefaultText,
            input.StomachText);
        if (interactionResult.IsError)
        {
            return interactionResult.Errors;
        }
        Interaction interaction = interactionResult.Value;

        // create the left party
        ErrorOr<InteractionParty> leftPartyResult = InteractionParty.Create(
            interaction.Id,
            input.LeftParty.PartyType,
            input.LeftParty.PartyId);
        if (leftPartyResult.IsError)
        {
            return leftPartyResult.Errors;
        }

        // create the right party
        ErrorOr<InteractionParty> rightPartyResult = InteractionParty.Create(
            interaction.Id,
            input.RightParty.PartyType,
            input.RightParty.PartyId);
        if (rightPartyResult.IsError)
        {
            return rightPartyResult.Errors;
        }

        // add the parties to the interaction
        interaction.AddParty(leftPartyResult.Value);
        interaction.AddParty(rightPartyResult.Value);

        // build the expanded pairs
        List<InteractionAtomicPair> expandedPairs = BuildExpandedPairs(input);

        // add the expanded pairs to the interaction
        interaction.ReplaceExpandedRows(expandedPairs.Select(pair =>
            InteractionExpanded.Create(
                interaction.Id,
                pair.PartyTypeA,
                pair.PartyIdA,
                pair.PartyTypeB,
                pair.PartyIdB)));

        // save the interaction to the database
        await _interactionRepository.AddAsync(interaction, cancellationToken);

        return new CreateInteractionWorkflowOutput(
            InteractionId: interaction.Id,
            DefaultText: interaction.DefaultText,
            StomachText: interaction.StomachText);
    }

    /// <summary>
    /// Builds the expanded pairs for the interaction.
    /// </summary>
    /// <param name="input">Input containing the interaction data.</param>
    /// <returns>List of expanded pairs.</returns>
    private static List<InteractionAtomicPair> BuildExpandedPairs(CreateInteractionWorkflowInput input)
    {
        var pairs = new List<InteractionAtomicPair>();

        foreach (Guid leftId in input.LeftAtomicIds)
        {
            foreach (Guid rightId in input.RightAtomicIds)
            {
                if (input.LeftAtomicType == input.RightAtomicType && leftId == rightId)
                {
                    continue;
                }

                InteractionAtomicPair pair = CanonicalizePair(
                    input.LeftAtomicType,
                    leftId,
                    input.RightAtomicType,
                    rightId);

                pairs.Add(pair);
            }
        }

        return pairs.Distinct().ToList();
    }

    private static InteractionAtomicPair CanonicalizePair(
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
