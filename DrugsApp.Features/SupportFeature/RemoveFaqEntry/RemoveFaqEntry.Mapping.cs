using System;

namespace DrugsApp.Features.SupportFeature.RemoveFaqEntry;

/// <summary>
/// Mapping helpers for the FAQ delete slice.
/// </summary>
public static class RemoveFaqEntryMapping
{
    /// <summary>
    /// Maps the request to the delete command.
    /// </summary>
    public static RemoveFaqEntryCommand MapToCommand(this RemoveFaqEntryRequest request) => new(Id: request.Id);
}
