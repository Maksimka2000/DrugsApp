using System;

namespace DrugsApp.Features.HealthConditionFeature.UpdateUserHealthConditionNote;

/// <summary>
/// Mapping helpers for updating user health condition note.
/// </summary>
public static class UpdateUserHealthConditionNoteMapping
{
    public static UpdateUserHealthConditionNoteCommand MapToCommand(
        this UpdateUserHealthConditionNoteRequest request,
        Guid userId,
        Guid id) => new(userId, id, request);
}
