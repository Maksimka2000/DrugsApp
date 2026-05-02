using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Features.SupplementFeature.Shared;

namespace DrugsApp.Features.SupplementFeature.UpdateSupplement;

/// <summary>
/// Mapping helpers for UpdateSupplement use case.
/// </summary>
public static class UpdateSupplementMapping
{
    public static UpdateSupplementCommand MapToCommand(
        this UpdateSupplementRequest request,
        Guid supplementId) => new(
            SupplementId: supplementId,
            Request: request);

    public static UpdatedSupplementResponse MapToResponse(this Supplement supplement) => new(
        Supplement: supplement.MapToDto());
}
