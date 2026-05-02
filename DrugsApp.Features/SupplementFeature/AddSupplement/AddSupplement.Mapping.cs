using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Features.SupplementFeature.Shared;

namespace DrugsApp.Features.SupplementFeature.AddSupplement;

/// <summary>
/// Mapping helpers for AddSupplement use case.
/// </summary>
public static class AddSupplementMapping
{
    public static AddSupplementCommand MapToCommand(this AddSupplementRequest request) => new(
        Request: request);

    public static Supplement MapToEntity(this AddSupplementRequest request) => Supplement.Create(
        name: request.Name,
        aliases: request.Aliases);

    public static CreatedSupplementResponse MapToResponse(this Supplement supplement) => new(
        Supplement: supplement.MapToDto());
}
