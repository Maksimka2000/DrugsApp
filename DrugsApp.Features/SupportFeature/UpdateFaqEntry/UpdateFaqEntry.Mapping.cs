using System;
using DrugsApp.Features.SupportFeature.Shared;

namespace DrugsApp.Features.SupportFeature.UpdateFaqEntry;

/// <summary>
/// Mapping helpers for the FAQ patch slice.
/// </summary>
public static class UpdateFaqEntryMapping
{
    /// <summary>
    /// Maps the HTTP request and route id to the update command.
    /// </summary>
    public static UpdateFaqEntryCommand MapToCommand(
        this UpdateFaqEntryRequest request,
        Guid id) => new(
        Id: id,
        Request: request);

    /// <summary>
    /// Maps the updated admin FAQ DTO to the response envelope.
    /// </summary>
    public static UpdatedFaqEntryResponse MapToResponse(this AdminFaqEntryDto faqEntry) => new(
        FaqEntry: faqEntry);
}
