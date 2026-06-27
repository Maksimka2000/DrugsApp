using System.Collections.Generic;
using System.Linq;
using DrugsApp.Domain.HealthConditionFeature.Entities;

namespace DrugsApp.Features.HealthConditionFeature.Shared;

/// <summary>
/// Mapping helpers for health condition shared DTOs.
/// </summary>
public static class HealthConditionMappings
{
    /// <summary>
    /// Maps a domain health condition entity to its DTO representation.
    /// </summary>
    /// <param name="healthCondition">Health condition entity to convert.</param>
    /// <returns>DTO populated from the entity.</returns>
    public static HealthConditionDto MapToDto(this HealthCondition healthCondition) => new(
        Id: healthCondition.Id,
        HealthConditionName: healthCondition.HealthConditionName,
        Description: healthCondition.Description,
        CreatedAtUtc: healthCondition.CreatedAtUtc,
        UpdatedAtUtc: healthCondition.UpdatedAtUtc);

    /// <summary>
    /// Maps a collection of health condition entities to a list of DTOs.
    /// </summary>
    /// <param name="healthConditions">Entities to convert.</param>
    /// <returns>List of DTOs.</returns>
    public static List<HealthConditionDto> MapToDtoList(this IEnumerable<HealthCondition> healthConditions) =>
        healthConditions.Select(healthCondition => healthCondition.MapToDto()).ToList();

    /// <summary>
    /// Maps a domain health condition entity to a summary DTO.
    /// </summary>
    /// <param name="healthCondition">Health condition entity to summarize.</param>
    /// <returns>Summary DTO.</returns>
    public static HealthConditionSummaryDto MapToSummaryDto(this HealthCondition healthCondition) => new(
        HealthConditionName: healthCondition.HealthConditionName,
        Description: healthCondition.Description);

    /// <summary>
    /// Maps a user health condition entity to its DTO representation, including derived display name.
    /// </summary>
    /// <param name="userHealthCondition">User health condition entity to convert.</param>
    /// <returns>User health condition DTO.</returns>
    public static UserHealthConditionDto MapToDto(this UserHealthCondition userHealthCondition)
    {
        bool isCatalog = userHealthCondition.Source == HealthConditionSource.Database;

        string displayName = isCatalog
            ? userHealthCondition.HealthCondition?.HealthConditionName ?? string.Empty
            : userHealthCondition.CustomHealthConditionName ?? string.Empty;

        HealthConditionSummaryDto? summary = isCatalog && userHealthCondition.HealthCondition is not null
            ? userHealthCondition.HealthCondition.MapToSummaryDto()
            : null;

        return new UserHealthConditionDto(
            Id: userHealthCondition.Id,
            Source: userHealthCondition.Source.ToString().ToLowerInvariant(),
            HealthConditionId: userHealthCondition.HealthConditionId,
            CustomHealthConditionName: userHealthCondition.CustomHealthConditionName,
            DisplayName: displayName,
            Note: userHealthCondition.Note,
            CreatedAtUtc: userHealthCondition.CreatedAtUtc,
            UpdatedAtUtc: userHealthCondition.UpdatedAtUtc,
            HealthConditionSummary: summary);
    }

    /// <summary>
    /// Maps a collection of user health condition entities to a list of DTOs.
    /// </summary>
    /// <param name="userHealthConditions">User health condition entities.</param>
    /// <returns>List of user health condition DTOs.</returns>
    public static List<UserHealthConditionDto> MapToDtoList(
        this IEnumerable<UserHealthCondition> userHealthConditions) =>
        userHealthConditions.Select(userHealthCondition => userHealthCondition.MapToDto()).ToList();
}
