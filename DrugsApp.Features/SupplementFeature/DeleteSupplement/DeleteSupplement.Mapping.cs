using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Features.SupplementFeature.Shared;
namespace DrugsApp.Features.SupplementFeature.DeleteSupplement;

/// <summary>
/// Mapping helpers for DeleteSupplement use case.
/// </summary>
public static class DeleteSupplementMapping
{
    /// <summary>
    /// Maps a unique identifier to a DeleteSupplementCommand instance.
    /// </summary>
    /// <param name="id">The unique identifier of the supplement to delete.</param>
    /// <returns>A new instance of <see cref="DeleteSupplementCommand"/> initialized with the given identifier.</returns>
    public static DeleteSupplementCommand MapToCommand(Guid id) => new(SupplementId: id);

    /// <summary>
    /// Maps a Supplement entity to a DeleteSupplementResponse instance.
    /// </summary>
    /// <param name="supplement">The supplement entity to be mapped.</param>
    /// <returns>A new instance of <see cref="DeleteSupplementResponse"/> containing the mapped supplement data.</returns>
    public static DeleteSupplementResponse MapToDeletedResponse(this Supplement supplement) => new(Supplement: supplement.MapToDto());
}
