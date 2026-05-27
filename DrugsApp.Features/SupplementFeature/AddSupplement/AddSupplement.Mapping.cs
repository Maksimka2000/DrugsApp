using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Features.SupplementFeature.Shared;

namespace DrugsApp.Features.SupplementFeature.AddSupplement;

/// <summary>
/// Mapping helpers for AddSupplement use case.
/// </summary>
public static class AddSupplementMapping
{
    /// <summary>
    /// Maps an <see cref="AddSupplementRequest"/> to an <see cref="AddSupplementCommand"/>.
    /// </summary>
    /// <param name="request">The request object containing details for adding a supplement.</param>
    /// <returns>An <see cref="AddSupplementCommand"/> containing the mapped data.</returns>
    public static AddSupplementCommand MapToCommand(this AddSupplementRequest request) => new(
        Request: request);

    /// <summary>
    /// Maps an <see cref="AddSupplementRequest"/> to a <see cref="Supplement"/> entity.
    /// </summary>
    /// <param name="request">The request object containing details for creating a supplement.</param>
    /// <returns>A <see cref="Supplement"/> entity populated with the provided data.</returns>
    public static Supplement MapToEntity(this AddSupplementRequest request) => Supplement.Create(
        name: request.Name,
        aliases: request.Aliases);

    /// <summary>
    /// Maps a <see cref="Supplement"/> entity to a <see cref="CreatedSupplementResponse"/>.
    /// </summary>
    /// <param name="supplement">The supplement entity containing the data to be mapped.</param>
    /// <returns>A <see cref="CreatedSupplementResponse"/> containing the mapped supplement data.</returns>
    public static CreatedSupplementResponse MapToResponse(this Supplement supplement) => new(
        Supplement: supplement.MapToDto());
}
