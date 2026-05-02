using System;
using DrugsApp.Domain.ActiveSubstanceGroupFeature.Entities;

namespace DrugsApp.Features.ActiveSubstanceGroupFeature.DeleteActiveSubstanceGroup;

/// <summary>
/// Mapping helpers for DeleteActiveSubstanceGroup use case.
/// </summary>
public static class DeleteActiveSubstanceGroupMapping
{
    /// <summary>
    /// Maps request to command.
    /// </summary>
    public static DeleteActiveSubstanceGroupCommand MapToCommand(this DeleteActiveSubstanceGroupRequest request) =>
        new(request.Id);

    /// <summary>
    /// Maps aggregate to delete response.
    /// </summary>
    public static DeleteActiveSubstanceGroupResponse MapToDeletedResponse(this ActiveSubstanceGroup group) => new(group.Id);
}
