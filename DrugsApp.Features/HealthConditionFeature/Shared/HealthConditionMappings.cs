using System.Collections.Generic;
using System.Linq;
using DrugsApp.Domain.HealthConditionFeature.Entities;

namespace DrugsApp.Features.HealthConditionFeature.Shared;

/// <summary>
/// Mapping helpers for health condition shared DTOs.
/// </summary>
public static class HealthConditionMappings
{
    public static HealthConditionDto MapToDto(this HealthCondition healthCondition) => new(
        Id: healthCondition.Id,
        HealthConditionName: healthCondition.HealthConditionName,
        Description: healthCondition.Description,
        CreatedAtUtc: healthCondition.CreatedAtUtc,
        UpdatedAtUtc: healthCondition.UpdatedAtUtc);

    public static List<HealthConditionDto> MapToDtoList(this IEnumerable<HealthCondition> healthConditions) =>
        healthConditions.Select(healthCondition => healthCondition.MapToDto()).ToList();

    public static HealthConditionSummaryDto MapToSummaryDto(this HealthCondition healthCondition) => new(
        HealthConditionName: healthCondition.HealthConditionName,
        Description: healthCondition.Description);

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

    public static List<UserHealthConditionDto> MapToDtoList(
        this IEnumerable<UserHealthCondition> userHealthConditions) =>
        userHealthConditions.Select(userHealthCondition => userHealthCondition.MapToDto()).ToList();
}
