using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Features.SupplementFeature.Shared;

namespace DrugsApp.Features.SupplementFeature.UpdateSupplement;

/// <summary>
/// Mapping helpers for UpdateSupplement use case.
/// </summary>
public static class UpdateSupplementMapping
{
    /// <summary>
    /// Maps an <see cref="UpdateSupplementRequest"/> to an <see cref="UpdateSupplementCommand"/>.
    /// </summary>
    /// <param name="request">The request object containing the fields to update.</param>
    /// <param name="supplementId">The unique identifier of the supplement to update.</param>
    /// <returns>An <see cref="UpdateSupplementCommand"/> instance with the provided request data and supplement ID.</returns>
    public static UpdateSupplementCommand MapToCommand(
        this UpdateSupplementRequest request,
        Guid supplementId) => new(
        SupplementId: supplementId,
        Request: request);

    /// <summary>
    /// Maps a <see cref="Supplement"/> entity to an <see cref="UpdatedSupplementResponse"/>.
    /// </summary>
    /// <param name="supplement">The supplement entity to be converted.</param>
    /// <returns>An <see cref="UpdatedSupplementResponse"/> instance containing the converted supplement data.</returns>
    public static UpdatedSupplementResponse MapToResponse(this Supplement supplement) => new(
        Supplement: supplement.MapToDto());
}
