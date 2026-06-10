using System;
using DrugsApp.Features.DrugProblemsFeature.Shared;

namespace DrugsApp.Features.DrugProblemsFeature.CreateDrugProblem;

/// <summary>
/// Mapping helpers for creating drug problems.
/// </summary>
public static class CreateDrugProblemMapping
{
    public static CreateDrugProblemCommand MapToCommand(this CreateDrugProblemRequest request, Guid userId) =>
        new(userId, request);

    public static CreateDrugProblemResponse MapToResponse(this DrugProblemDetailsDto item) =>
        new(item);
}
