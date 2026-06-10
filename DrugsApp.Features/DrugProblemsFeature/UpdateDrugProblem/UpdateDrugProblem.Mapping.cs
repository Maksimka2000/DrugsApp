using System;
using DrugsApp.Features.DrugProblemsFeature.Shared;

namespace DrugsApp.Features.DrugProblemsFeature.UpdateDrugProblem;

/// <summary>
/// Mapping helpers for updating drug problems.
/// </summary>
public static class UpdateDrugProblemMapping
{
    public static UpdateDrugProblemCommand MapToCommand(this UpdateDrugProblemRequest request, Guid userId, Guid id) =>
        new(userId, id, request);

    public static UpdateDrugProblemResponse MapToResponse(this DrugProblemDetailsDto item) =>
        new(item);
}
