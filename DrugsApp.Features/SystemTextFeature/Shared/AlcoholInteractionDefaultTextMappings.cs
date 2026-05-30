using DrugsApp.Domain.SystemTextFeature.Entities;

namespace DrugsApp.Features.SystemTextFeature.Shared;

/// <summary>
/// Mapping helpers for alcohol interaction default text.
/// </summary>
public static class AlcoholInteractionDefaultTextMappings
{
    /// <summary>
    /// Maps a <see cref="SystemText"/> entity to an <see cref="AlcoholInteractionDefaultTextDto"/>.
    /// </summary>
    /// <param name="systemText">
    /// The <see cref="SystemText"/> entity containing the alcohol interaction default text information.
    /// </param>
    /// <returns>
    /// An <see cref="AlcoholInteractionDefaultTextDto"/> containing the mapped default text.
    /// </returns>
    public static AlcoholInteractionDefaultTextDto MapToDto(this SystemText systemText) =>
        new(systemText.Value);
}
