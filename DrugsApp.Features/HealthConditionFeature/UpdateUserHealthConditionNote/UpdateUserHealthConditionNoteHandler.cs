using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.HealthConditionFeature.Entities;
using DrugsApp.Domain.HealthConditionFeature.Repositories;
using DrugsApp.Features.HealthConditionFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.HealthConditionFeature.UpdateUserHealthConditionNote;

/// <summary>
/// Handles note updates for user health conditions.
/// </summary>
public sealed class UpdateUserHealthConditionNoteHandler(
    IUserHealthConditionRepository _userHealthConditionRepository)
    : IRequestHandler<UpdateUserHealthConditionNoteCommand, ErrorOr<UserHealthConditionDto>>
{
    /// <summary>
    /// Updates the note for a user's health condition.
    /// </summary>
    /// <param name="command">Command containing user id, condition id, and note payload.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Updated user health condition DTO or errors.</returns>
    public async Task<ErrorOr<UserHealthConditionDto>> Handle(
        UpdateUserHealthConditionNoteCommand command,
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

        ErrorOr<Success> updateResult = userHealthCondition.UpdateNote(command.Request.Note);
        if (updateResult.IsError)
        {
            return updateResult.Errors;
        }

        return userHealthCondition.MapToDto();
    }
}
