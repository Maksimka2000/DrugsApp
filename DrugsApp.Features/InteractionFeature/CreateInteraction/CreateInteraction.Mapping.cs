namespace DrugsApp.Features.InteractionFeature.CreateInteraction;

/// <summary>
/// Provides mapping functionalities for interaction creation-related objects in the InteractionFeature namespace.
/// </summary>
public static class CreateInteractionMapping
{
    /// <summary>
    /// Maps a <see cref="CreateInteractionRequest"/> to a <see cref="CreateInteractionCommand"/>.
    /// </summary>
    /// <param name="request">The request object containing the interaction details to be mapped.</param>
    /// <returns>A new instance of <see cref="CreateInteractionCommand"/> populated with data from the provided request.</returns>
    public static CreateInteractionCommand MapToCommand(this CreateInteractionRequest request) =>
        new(request);
}
