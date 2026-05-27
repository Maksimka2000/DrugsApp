using System;

namespace DrugsApp.Features.InteractionFeature.UpdateInteraction;

/// <summary>
/// Defines a collection of mapping logic related to the "Update Interaction" feature
/// within the application. This class provides methods to facilitate the transformation
/// of data structures for processing interaction updates.
/// </summary>
public static class UpdateInteractionMapping
{
    /// <summary>
    /// Maps an <see cref="UpdateInteractionRequest"/> to an <see cref="UpdateInteractionCommand"/>.
    /// </summary>
    /// <param name="request">
    /// The request object containing the details of the interaction to be updated.
    /// </param>
    /// <param name="interactionId">
    /// The unique identifier of the interaction to be updated.
    /// </param>
    /// <returns>
    /// An instance of <see cref="UpdateInteractionCommand"/> that contains the interaction ID
    /// and the update request details.
    /// </returns>
    public static UpdateInteractionCommand MapToCommand(
        this UpdateInteractionRequest request,
        Guid interactionId) =>
        new(interactionId, request);
}
