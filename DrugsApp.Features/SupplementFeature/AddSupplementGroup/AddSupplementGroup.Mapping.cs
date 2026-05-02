using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Features.SupplementFeature.Shared;

namespace DrugsApp.Features.SupplementFeature.AddSupplementGroup;

/// <summary>
/// Mapping helpers for AddSupplementGroup use case.
/// </summary>
public static class AddSupplementGroupMapping
{
    public static AddSupplementGroupCommand MapToCommand(this AddSupplementGroupRequest request) => new(Request: request);

    public static SupplementGroup MapToEntity(this AddSupplementGroupRequest request) => SupplementGroup.Create(
        name: request.Name);

    public static CreatedSupplementGroupResponse MapToResponse(this SupplementGroup group) => new(SupplementGroup: group.MapToDto());


}
