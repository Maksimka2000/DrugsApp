using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.HealthConditionFeature.Entities;
using DrugsApp.Domain.HealthConditionFeature.Repositories;
using DrugsApp.Features.HealthConditionFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.HealthConditionFeature.UpdateHealthCondition;

/// <summary>
/// Handles updates to catalog health conditions.
/// </summary>
public sealed class UpdateHealthConditionHandler(
    IHealthConditionRepository _healthConditionRepository)
    : IRequestHandler<UpdateHealthConditionCommand, ErrorOr<HealthConditionDto>>
{
    public async Task<ErrorOr<HealthConditionDto>> Handle(
        UpdateHealthConditionCommand command,
        CancellationToken cancellationToken)
    {
        HealthCondition? healthCondition = await _healthConditionRepository.GetByIdAsync(
            command.Id,
            cancellationToken);
        if (healthCondition is null)
        {
            return Error.NotFound(
                description: "Health condition not found.",
                metadata: new()
                {
                    ["resourceId"] = command.Id
                });
        }

        string? normalizedName = NormalizeName(command.Request.HealthConditionName);
        if (normalizedName is not null)
        {
            HealthCondition? existing = await _healthConditionRepository.GetByNormalizedNameAsync(
                normalizedName,
                cancellationToken);
            if (existing is not null && existing.Id != command.Id)
            {
                return Error.Conflict(
                    description: "Health condition name already exists.",
                    metadata: new()
                    {
                        ["conflictingResourceId"] = existing.Id
                    });
            }
        }

        ErrorOr<Success> updateResult = healthCondition.Update(
            command.Request.HealthConditionName,
            command.Request.Description);
        if (updateResult.IsError)
        {
            return updateResult.Errors;
        }

        return healthCondition.MapToDto();
    }

    private static string? NormalizeName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string trimmed = value.Trim();
        return string.Join(
                ' ',
                trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            .ToLowerInvariant();
    }
}
