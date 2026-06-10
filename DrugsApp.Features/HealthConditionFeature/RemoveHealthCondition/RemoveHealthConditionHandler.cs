using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.HealthConditionFeature.Entities;
using DrugsApp.Domain.HealthConditionFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.HealthConditionFeature.RemoveHealthCondition;

/// <summary>
/// Handles removal of catalog health conditions.
/// </summary>
public sealed class RemoveHealthConditionHandler(
    IHealthConditionRepository _healthConditionRepository)
    : IRequestHandler<RemoveHealthConditionCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(
        RemoveHealthConditionCommand command,
        CancellationToken cancellationToken)
    {
        HealthCondition? healthCondition = await _healthConditionRepository.GetByIdAsync(
            command.Id,
            cancellationToken);

        if (healthCondition is null)
        {
            return Error.NotFound(
                description: "Health condition not found.",
                metadata: new Dictionary<string, object>
                {
                    ["resourceId"] = command.Id
                });
        }

        _healthConditionRepository.Remove(healthCondition);

        return Result.Success;
    }
}
