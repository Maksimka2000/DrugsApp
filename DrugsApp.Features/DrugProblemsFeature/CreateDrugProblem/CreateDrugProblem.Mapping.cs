using System;
using DrugsApp.Features.DrugProblemsFeature.Shared;

namespace DrugsApp.Features.DrugProblemsFeature.CreateDrugProblem;

/// <summary>
/// Mapping helpers for creating drug problems.
/// </summary>
public static class CreateDrugProblemMapping
{
    /// <summary>
    /// Builds a create command with the current user context.
    /// </summary>
    /// <param name="request">Incoming request payload.</param>
    /// <param name="userId">Identifier of the user creating the problem.</param>
    /// <returns>Command carrying user id and request body.</returns>
    public static CreateDrugProblemCommand MapToCommand(this CreateDrugProblemRequest request, Guid userId) =>
        new(userId, request);

    /// <summary>
    /// Maps the created drug problem details into the response DTO.
    /// </summary>
    /// <param name="item">Created drug problem details.</param>
    /// <returns>Response DTO wrapping the created item.</returns>
    public static CreateDrugProblemResponse MapToResponse(this DrugProblemDetailsDto item) =>
        new(item);
}
