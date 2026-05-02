using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Features.SupplementFeature.Shared;

namespace DrugsApp.Features.SupplementFeature.UpdateSupplementGroup;

/// <summary>
/// Mapping helpers for UpdateSupplementGroup use case.
/// </summary>
public static class UpdateSupplementGroupMapping
{
    public static UpdateSupplementGroupCommand MapToCommand(
        this UpdateSupplementGroupRequest request,
        Guid id) => new(
            SupplementGroupId: id,
            Request: request);

    public static UpdatedSupplementGroupResponse MapToResponse(this SupplementGroup group) => new(
        SupplementGroup: group.MapToDto());
}
