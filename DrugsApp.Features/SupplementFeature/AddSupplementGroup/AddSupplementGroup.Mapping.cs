using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Features.SupplementFeature.Shared;

namespace DrugsApp.Features.SupplementFeature.AddSupplementGroup;

/// <summary>
/// Mapping helpers for AddSupplementGroup use case.
/// </summary>
public static class AddSupplementGroupMapping
{
    /// <summary>
    /// Maps an <see cref="AddSupplementGroupRequest"/> instance to an <see cref="AddSupplementGroupCommand"/> object.
    /// </summary>
    /// <param name="request">The <see cref="AddSupplementGroupRequest"/> instance to be mapped.</param>
    /// <returns>An <see cref="AddSupplementGroupCommand"/> object representing the mapped request.</returns>
    public static AddSupplementGroupCommand MapToCommand(this AddSupplementGroupRequest request) => new(Request: request);

    /// <summary>
    /// Maps an <see cref="AddSupplementGroupRequest"/> to a <see cref="SupplementGroup"/> entity.
    /// </summary>
    /// <param name="request">The <see cref="AddSupplementGroupRequest"/> containing the data to create a <see cref="SupplementGroup"/> entity.</param>
    /// <returns>A <see cref="SupplementGroup"/> entity created from the provided request.</returns>
    public static SupplementGroup MapToEntity(this AddSupplementGroupRequest request) => SupplementGroup.Create(
        name: request.Name);

    /// <summary>
    /// Maps a <see cref="SupplementGroup"/> entity to a <see cref="CreatedSupplementGroupResponse"/> object.
    /// </summary>
    /// <param name="group">The <see cref="SupplementGroup"/> entity to be mapped.</param>
    /// <returns>A <see cref="CreatedSupplementGroupResponse"/> object representing the mapped entity.</returns>
    public static CreatedSupplementGroupResponse MapToResponse(this SupplementGroup group) => new(SupplementGroup: group.MapToDto());


}
