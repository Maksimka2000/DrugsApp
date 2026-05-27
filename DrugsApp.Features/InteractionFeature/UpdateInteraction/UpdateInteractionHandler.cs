using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.InteractionFeature.Entities;
using DrugsApp.Domain.InteractionFeature.Repositories;
using DrugsApp.Features.InteractionFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.InteractionFeature.UpdateInteraction;

/// <summary>
/// Represents the command to update an existing interaction.
/// </summary>
/// <param name="InteractionId">
/// The unique identifier of the interaction to be updated.
/// </param>
/// <param name="Request">
/// The request payload containing the update details for the interaction,
/// including optional text fields to modify and severity information.
/// </param>
public sealed record UpdateInteractionCommand(
    Guid InteractionId,
    UpdateInteractionRequest Request) : IRequest<ErrorOr<UpdatedInteractionResponse>>;

/// <summary>
/// Represents the response returned after successfully updating an interaction.
/// </summary>
/// <param name="InteractionId">
/// The unique identifier of the updated interaction.
/// </param>
/// <param name="Severity">
/// The severity details of the interaction, including its identifier, severity number, name,
/// and optional description.
/// </param>
/// <param name="DefaultText">
/// The text related to the interaction's default condition, or null if not set.
/// </param>
/// <param name="StomachText">
/// The text related to the interaction's effect on the stomach, or null if not set.
/// </param>
public sealed record UpdatedInteractionResponse(
    Guid InteractionId,
    InteractionSeverityDto Severity,
    string? DefaultText,
    string? StomachText);

internal sealed class UpdateInteractionHandler(
    IInteractionRepository _interactionRepository)
    : IRequestHandler<UpdateInteractionCommand, ErrorOr<UpdatedInteractionResponse>>
{
    public async Task<ErrorOr<UpdatedInteractionResponse>> Handle(
        UpdateInteractionCommand command,
        CancellationToken cancellationToken)
    {
        // get interaction by id
        Interaction? interaction = await _interactionRepository.GetByIdAsync(command.InteractionId, cancellationToken);
        if (interaction is null)
        {
            return Error.NotFound(
                description: "Interaction not found.",
                metadata: new Dictionary<string, object>
                {
                    ["resourceType"] = "interaction",
                    ["resourceId"] = command.InteractionId.ToString()
                });
        }

        InteractionSeverity? severity = interaction.InteractionSeverity;

        if (command.Request.SeverityId is not null)
        {
            InteractionSeverity? severityEntity = await _interactionRepository.GetInteractionSeverityByIdAsync(
                command.Request.SeverityId,
                cancellationToken);

            if (severityEntity is null)
            {
                return Error.NotFound(
                    description: "Interaction severity not found.",
                    metadata: new Dictionary<string, object>
                    {
                        ["resourceType"] = "interactionSeverity",
                        ["resourceId"] = command.Request.SeverityId.ToString() ?? string.Empty
                    });
            }

            interaction.UpdateSeverity(severityEntity.Id);
            severity = severityEntity;
        }

        ErrorOr<Success> updateTextsResult = interaction.UpdateTexts(
            command.Request.DefaultText,
            command.Request.StomachText,
            command.Request.ClearDefaultText ?? false,
            command.Request.ClearStomachText ?? false);

        if (updateTextsResult.IsError)
        {
            return updateTextsResult.Errors;
        }

        return new UpdatedInteractionResponse(
            InteractionId: interaction.Id,
            Severity: severity.MapToDto(),
            DefaultText: interaction.DefaultText,
            StomachText: interaction.StomachText);
    }
}
