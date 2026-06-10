using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.HealthConditionFeature.Entities;
using DrugsApp.Domain.HealthConditionFeature.Repositories;
using DrugsApp.Features.HealthConditionFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.HealthConditionFeature.AddUserHealthCondition;

/// <summary>
/// Handles creation of user health conditions.
/// </summary>
public sealed class AddUserHealthConditionHandler(
    IUserHealthConditionRepository _userHealthConditionRepository,
    IHealthConditionRepository _healthConditionRepository)
    : IRequestHandler<AddUserHealthConditionCommand, ErrorOr<UserHealthConditionDto>>
{
    public async Task<ErrorOr<UserHealthConditionDto>> Handle(
        AddUserHealthConditionCommand command,
        CancellationToken cancellationToken)
    {
        if (command.Request.HealthConditionId is Guid healthConditionId)
        {
            return await CreateDatabaseAsync(command.UserId, healthConditionId, command.Request.Note, cancellationToken);
        }

        return await CreateCustomAsync(command.UserId, command.Request.CustomHealthConditionName!, command.Request.Note, cancellationToken);
    }

    private async Task<ErrorOr<UserHealthConditionDto>> CreateDatabaseAsync(
        Guid userId,
        Guid healthConditionId,
        string? note,
        CancellationToken cancellationToken)
    {
        HealthCondition? healthCondition = await _healthConditionRepository.GetByIdAsync(
            healthConditionId,
            cancellationToken);
        if (healthCondition is null)
        {
            return Error.NotFound(
                description: "Health condition not found.",
                metadata: new()
                {
                    ["resourceId"] = healthConditionId
                });
        }

        bool existsCatalog = await _userHealthConditionRepository.ExistsCatalogConditionForUserAsync(
            userId,
            healthConditionId,
            cancellationToken);
        if (existsCatalog)
        {
            return Error.Conflict(
                description: "Health condition already saved for this user.",
                metadata: new()
                {
                    ["conflictingResourceId"] = healthConditionId
                });
        }

        bool existsCrossSourceDuplicate = await _userHealthConditionRepository.ExistsDuplicateForUserAsync(
            userId,
            healthCondition.NormalizedHealthConditionName,
            cancellationToken);
        if (existsCrossSourceDuplicate)
        {
            return Error.Conflict(
                description: "A health condition with the same name already exists for this user.",
                metadata: new()
                {
                    ["conflictingName"] = healthCondition.HealthConditionName
                });
        }

        ErrorOr<UserHealthCondition> created = UserHealthCondition.CreateDatabase(
            userId,
            healthConditionId,
            note);
        if (created.IsError)
        {
            return created.Errors;
        }

        created.Value.AttachHealthCondition(healthCondition);

        await _userHealthConditionRepository.AddAsync(created.Value, cancellationToken);

        return created.Value.MapToDto();
    }

    private async Task<ErrorOr<UserHealthConditionDto>> CreateCustomAsync(
        Guid userId,
        string customHealthConditionName,
        string? note,
        CancellationToken cancellationToken)
    {
        string normalizedCustomName = NormalizeName(customHealthConditionName);

        bool existsDuplicate = await _userHealthConditionRepository.ExistsDuplicateForUserAsync(
            userId,
            normalizedCustomName,
            cancellationToken);
        if (existsDuplicate)
        {
            return Error.Conflict(
                description: "A health condition with the same name already exists for this user.",
                metadata: new()
                {
                    ["field"] = "customHealthConditionName"
                });
        }

        ErrorOr<UserHealthCondition> created = UserHealthCondition.CreateCustom(
            userId,
            customHealthConditionName,
            note);
        if (created.IsError)
        {
            return created.Errors;
        }

        await _userHealthConditionRepository.AddAsync(created.Value, cancellationToken);

        return created.Value.MapToDto();
    }

    private static string NormalizeName(string value)
    {
        string trimmed = value.Trim();
        return string.Join(
                ' ',
                trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            .ToLowerInvariant();
    }
}
