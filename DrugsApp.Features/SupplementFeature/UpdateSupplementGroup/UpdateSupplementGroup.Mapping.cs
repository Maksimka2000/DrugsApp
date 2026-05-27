using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Features.SupplementFeature.Shared;

namespace DrugsApp.Features.SupplementFeature.UpdateSupplementGroup;

/// <summary>
/// Mapping helpers for UpdateSupplementGroup use case.
/// </summary>
public static class UpdateSupplementGroupMapping
{
    /// <summary>
    /// Maps an <see cref="UpdateSupplementGroupRequest"/> object to an instance of <see cref="UpdateSupplementGroupCommand"/>.
    /// </summary>
    /// <param name="request">The request object containing fields to update the supplement group.</param>
    /// <param name="id">The unique identifier of the supplement group to update.</param>
    /// <returns>A command instance encapsulating the update operation details.</returns>
    public static UpdateSupplementGroupCommand MapToCommand(
        this UpdateSupplementGroupRequest request,
        Guid id) => new(
        SupplementGroupId: id,
            Request: request);

    /// <summary>
    /// Maps a SupplementGroup entity to an UpdatedSupplementGroupResponse object.
    /// </summary>
    /// <param name="group">The SupplementGroup entity to map.</param>
    /// <returns>An UpdatedSupplementGroupResponse object containing the mapped data.</returns>
    public static UpdatedSupplementGroupResponse MapToResponse(this SupplementGroup group) => new(
        SupplementGroup: group.MapToDto());
}
