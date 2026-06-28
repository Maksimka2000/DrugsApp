using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Features.ActiveSubstanceFeature.Shared;

namespace DrugsApp.Features.ActiveSubstanceFeature.DeleteActiveSubstance;

/// <summary>
/// Mapping helpers for DeleteActiveSubstance use case.
/// </summary>
public static class DeleteActiveSubstanceMapping
{
    /// <summary>
    /// Maps a unique identifier to a DeleteActiveSubstanceCommand instance.
    /// </summary>
    public static DeleteActiveSubstanceCommand MapToCommand(Guid id) => new(ActiveSubstanceId: id);

    /// <summary>
    /// Maps an ActiveSubstance entity to a DeleteActiveSubstanceResponse instance.
    /// </summary>
    public static DeleteActiveSubstanceResponse MapToDeletedResponse(this ActiveSubstance activeSubstance) =>
        new(ActiveSubstance: activeSubstance.MapToDto());
}
