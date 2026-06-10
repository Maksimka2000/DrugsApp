using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.HealthConditionFeature.Entities;
using DrugsApp.Domain.HealthConditionFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.HealthConditionFeature.RemoveUserHealthCondition;

/// <summary>
/// Handles removal of user health conditions.
/// </summary>
public sealed class RemoveUserHealthConditionHandler(
    IUserHealthConditionRepository _userHealthConditionRepository)
    : IRequestHandler<RemoveUserHealthConditionCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(
        RemoveUserHealthConditionCommand command,
        CancellationToken cancellationToken)
    {
        UserHealthCondition? userHealthCondition = await _userHealthConditionRepository.GetByIdAsync(
            command.Id,
            cancellationToken);

        if (userHealthCondition is null || userHealthCondition.UserId != command.UserId)
        {
            return Error.NotFound(
                description: "User health condition not found.",
                metadata: new Dictionary<string, object>
                {
                    ["resourceId"] = command.Id
                });
        }

        _userHealthConditionRepository.Remove(userHealthCondition);

        return Result.Success;
    }
}
