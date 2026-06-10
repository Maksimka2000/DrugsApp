using System;

namespace DrugsApp.Features.DrugProblemsFeature.Shared;

/// <summary>
/// DTO representing a drug problem type dictionary entry.
/// </summary>
public sealed record DrugProblemTypeDto(
    Guid Id,
    string ProblemTypeName,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public static class DrugProblemTypeMappings
{
    public static DrugProblemTypeDto MapToDto(this DrugsApp.Domain.DrugProblemsFeature.Entities.DrugProblemType entity) =>
        new(entity.Id, entity.ProblemTypeName, entity.CreatedAtUtc, entity.UpdatedAtUtc);
}
