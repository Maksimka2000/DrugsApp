using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Features.SupplementFeature.Shared;

namespace DrugsApp.Features.SupplementFeature.DeleteSupplementGroup;

/// <summary>
/// Mapping helpers for DeleteSupplementGroup use case.
/// </summary>
public static class DeleteSupplementGroupMapping
{
    /// <summary>
    /// Maps the given identifier to a <see cref="DeleteSupplementGroupCommand"/>.
    /// </summary>
    /// <param name="id">The unique identifier of the supplement group to be deleted.</param>
    /// <returns>A <see cref="DeleteSupplementGroupCommand"/> initialized with the provided identifier.</returns>
    public static DeleteSupplementGroupCommand MapToCommand(Guid id) => new(SupplementGroupId: id);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="group"></param>
    /// <returns></returns>
    public static DeleteSupplementGroupResponse MapToDeletedResponse(this SupplementGroup group) => new(SupplementGroup: group.MapToDto());
}
