using System;

namespace DrugsApp.Features.HealthConditionFeature.UpdateUserHealthConditionNote;

/// <summary>
/// Mapping helpers for updating user health condition note.
/// </summary>
public static class UpdateUserHealthConditionNoteMapping
{
    /// <summary>
    /// Builds a command to update a user's health condition note.
    /// </summary>
    /// <param name="request">Note update payload.</param>
    /// <param name="userId">User identifier.</param>
    /// <param name="id">User health condition identifier.</param>
    /// <returns>Update note command.</returns>
    public static UpdateUserHealthConditionNoteCommand MapToCommand(
        this UpdateUserHealthConditionNoteRequest request,
        Guid userId,
        Guid id) => new(userId, id, request);
}
