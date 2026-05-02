using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Features.SupplementFeature.Shared;

namespace DrugsApp.Features.SupplementFeature.DeleteSupplementGroup;

/// <summary>
/// Mapping helpers for DeleteSupplementGroup use case.
/// </summary>
public static class DeleteSupplementGroupMapping
{
    public static DeleteSupplementGroupCommand MapToCommand(Guid id) => new(SupplementGroupId: id);

    public static DeleteSupplementGroupResponse MapToDeletedResponse(this SupplementGroup group) => new(SupplementGroup: group.MapToDto());
}
