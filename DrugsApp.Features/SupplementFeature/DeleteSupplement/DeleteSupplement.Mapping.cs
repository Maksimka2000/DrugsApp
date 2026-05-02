using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Features.SupplementFeature.Shared;
namespace DrugsApp.Features.SupplementFeature.DeleteSupplement;

/// <summary>
/// Mapping helpers for DeleteSupplement use case.
/// </summary>
public static class DeleteSupplementMapping
{
    public static DeleteSupplementCommand MapToCommand(Guid id) => new( SupplementId: id);

    public static DeleteSupplementResponse MapToDeletedResponse(this Supplement supplement) => new(Supplement: supplement.MapToDto());
}
