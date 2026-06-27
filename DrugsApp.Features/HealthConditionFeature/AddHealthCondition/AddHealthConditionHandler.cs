using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.HealthConditionFeature.Entities;
using DrugsApp.Domain.HealthConditionFeature.Repositories;
using DrugsApp.Features.HealthConditionFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.HealthConditionFeature.AddHealthCondition;

/// <summary>
/// Handles creation of catalog health conditions.
/// </summary>
public sealed class AddHealthConditionHandler(
    IHealthConditionRepository _healthConditionRepository)
    : IRequestHandler<AddHealthConditionCommand, ErrorOr<HealthConditionDto>>
{
    /// <summary>
    /// Creates a new catalog health condition if the normalized name is unique.
    /// </summary>
    /// <param name="command">Command containing name and description.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Created health condition DTO or errors.</returns>
    public async Task<ErrorOr<HealthConditionDto>> Handle(
        AddHealthConditionCommand command,
        CancellationToken cancellationToken)
    {
        string normalizedName = NormalizeName(command.Request.HealthConditionName);

        HealthCondition? existing = await _healthConditionRepository.GetByNormalizedNameAsync(
            normalizedName,
            cancellationToken);

        if (existing is not null)
        {
            return Error.Conflict(
                description: "Health condition name already exists.",
                metadata: new()
                {
                    ["conflictingResourceId"] = existing.Id
                });
        }

        ErrorOr<HealthCondition> created = HealthCondition.Create(
            command.Request.HealthConditionName,
            command.Request.Description);
        if (created.IsError)
        {
            return created.Errors;
        }

        await _healthConditionRepository.AddAsync(created.Value, cancellationToken);

        return created.Value.MapToDto();
    }

    private static string NormalizeName(string value)
    {
        string trimmed = value.Trim();
        return string.Join(
                ' ',
                trimmed.Split((char[]?)null, System.StringSplitOptions.RemoveEmptyEntries))
            .ToLowerInvariant();
    }
}
