using DrugsApp.Features.SupportFeature.Shared;

namespace DrugsApp.Features.SupportFeature.AddFaqEntry;

/// <summary>
/// Mapping helpers for the FAQ create slice.
/// </summary>
public static class AddFaqEntryMapping
{
    /// <summary>
    /// Maps the HTTP request to the create command.
    /// </summary>
    public static AddFaqEntryCommand MapToCommand(this AddFaqEntryRequest request) => new(
        Request: request);

    /// <summary>
    /// Maps the created FAQ DTO to the response envelope.
    /// </summary>
    public static CreatedFaqEntryResponse MapToResponse(this FaqEntryDto faqEntry) => new(
        FaqEntry: faqEntry);
}
