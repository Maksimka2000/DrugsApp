using DrugsApp.Domain.SystemTextFeature.Entities;
using DrugsApp.Features.SystemTextFeature.Shared;

namespace DrugsApp.Features.SystemTextFeature.AlcoholInteractionDefaultText;

/// <summary>
/// Mapping helpers for alcohol interaction default text use cases.
/// </summary>
public static class AlcoholInteractionDefaultTextMapping
{
    /// <summary>
    /// Maps an <see cref="UpdateAlcoholInteractionDefaultTextRequest"/> instance to an <see cref="UpdateAlcoholInteractionDefaultTextCommand"/>.
    /// </summary>
    /// <param name="request">The request containing the default text to update.</param>
    /// <returns>A command representing the update for the alcohol interaction default text.</returns>
    public static UpdateAlcoholInteractionDefaultTextCommand MapToCommand(
        this UpdateAlcoholInteractionDefaultTextRequest request) =>
        new(request.DefaultText!);

    /// <summary>
    /// Maps a <see cref="SystemText"/> entity to an <see cref="AlcoholInteractionDefaultTextDto"/>.
    /// </summary>
    /// <param name="systemText">The system text entity to map from.</param>
    /// <returns>A DTO representing the alcohol interaction default text.</returns>
    public static AlcoholInteractionDefaultTextDto MapToDto(this SystemText systemText) => new(DefaultText: systemText.Value);


}
