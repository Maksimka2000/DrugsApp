using System;

namespace DrugsApp.Features.DrugProblemsFeature.UpdateDrugProblemType;

/// <summary>
/// Mapping helpers for updating drug problem types.
/// </summary>
public static class UpdateDrugProblemTypeMapping
{
    /// <summary>
    /// Creates a command to update a drug problem type.
    /// </summary>
    /// <param name="request">Request payload containing updated fields.</param>
    /// <param name="id">Identifier of the drug problem type to update.</param>
    /// <returns>Command carrying id and payload.</returns>
    public static UpdateDrugProblemTypeCommand MapToCommand(this UpdateDrugProblemTypeRequest request, Guid id) =>
        new(id, request);
}
