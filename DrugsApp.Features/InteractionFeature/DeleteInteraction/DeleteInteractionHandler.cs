using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.InteractionFeature.Entities;
using DrugsApp.Domain.InteractionFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.InteractionFeature.DeleteInteraction;

/// <summary>
/// Represents a command to delete an interaction identified by its unique identifier.
/// This command is intended to be processed by a corresponding request handler to
/// remove interaction data from the persistence store.
/// </summary>
/// <param name="InteractionId">
/// The unique identifier of the interaction to be deleted.
/// </param>
/// <remarks>
/// The deletion process involves locating the interaction using the provided identifier
/// and removing it if it exists. If no interaction is found with the given identifier,
/// the operation may result in an error indicating the entity was not found.
/// </remarks>
public sealed record DeleteInteractionCommand(Guid InteractionId) : IRequest<ErrorOr<Success>>;

internal sealed class DeleteInteractionHandler(IInteractionRepository _interactionRepository)
    : IRequestHandler<DeleteInteractionCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(
        DeleteInteractionCommand request,
        CancellationToken cancellationToken)
    {
        Interaction? interaction = await _interactionRepository.GetByIdAsync(
            request.InteractionId,
            cancellationToken);

        if (interaction is null)
        {
            return Error.NotFound(
                description: "Interaction not found.",
                metadata: new Dictionary<string, object>
                {
                    ["resourceType"] = "interaction",
                    ["resourceId"] = request.InteractionId.ToString()
                });
        }

        _interactionRepository.Remove(interaction);
        return Result.Success;
    }
}
