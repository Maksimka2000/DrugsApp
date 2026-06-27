using System;

namespace DrugsApp.Features.DrugProblemsFeature.Shared;

/// <summary>
/// DTO representing a drug problem type dictionary entry.
/// </summary>
/// <param name="Id">Identifier of the drug problem type.</param>
/// <param name="ProblemTypeName">Display name of the problem type.</param>
/// <param name="CreatedAtUtc">Creation timestamp.</param>
/// <param name="UpdatedAtUtc">Last update timestamp.</param>
public sealed record DrugProblemTypeDto(
    Guid Id,
    string ProblemTypeName,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

/// <summary>
/// Mapping helpers for drug problem type dictionary.
/// </summary>
public static class DrugProblemTypeMappings
{
    /// <summary>
    /// Converts a domain drug problem type entity to its DTO representation.
    /// </summary>
    /// <param name="entity">Domain drug problem type entity.</param>
    /// <returns>DTO populated from the entity.</returns>
    public static DrugProblemTypeDto MapToDto(this DrugsApp.Domain.DrugProblemsFeature.Entities.DrugProblemType entity) =>
        new(entity.Id, entity.ProblemTypeName, entity.CreatedAtUtc, entity.UpdatedAtUtc);
}
