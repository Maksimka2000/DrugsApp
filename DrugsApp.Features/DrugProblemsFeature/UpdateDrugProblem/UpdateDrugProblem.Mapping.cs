using System;
using DrugsApp.Features.DrugProblemsFeature.Shared;

namespace DrugsApp.Features.DrugProblemsFeature.UpdateDrugProblem;

/// <summary>
/// Mapping helpers for updating drug problems.
/// </summary>
public static class UpdateDrugProblemMapping
{
    /// <summary>
    /// Builds an update command from the request and user context.
    /// </summary>
    /// <param name="request">Update payload.</param>
    /// <param name="userId">Identifier of the user owning the problem.</param>
    /// <param name="id">Identifier of the problem being updated.</param>
    /// <returns>Command carrying the identifiers and payload.</returns>
    public static UpdateDrugProblemCommand MapToCommand(this UpdateDrugProblemRequest request, Guid userId, Guid id) =>
        new(userId, id, request);

    /// <summary>
    /// Maps updated drug problem details to response DTO.
    /// </summary>
    /// <param name="item">Updated drug problem details.</param>
    /// <returns>Response wrapping the updated item.</returns>
    public static UpdateDrugProblemResponse MapToResponse(this DrugProblemDetailsDto item) =>
        new(item);
}
