using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.RegularSupplementsFeature.Entities;
using DrugsApp.Domain.RegularSupplementsFeature.Repositories;
using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Domain.SupplementFeature.Repositories;
using DrugsApp.Features.RegularSupplementsFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.RegularSupplementsFeature.UpdateOwnSupplement;

/// <summary>
/// Handles updating own supplement compounds.
/// </summary>
internal sealed class UpdateOwnSupplementHandler(
    ISupplementRepository _supplementRepository,
    IClientOwnSupplementRepository _clientOwnSupplementRepository)
    : IRequestHandler<UpdateOwnSupplementCommand, ErrorOr<UpdateOwnSupplementResponse>>
{
    public async Task<ErrorOr<UpdateOwnSupplementResponse>> Handle(
        UpdateOwnSupplementCommand command,
        CancellationToken cancellationToken)
    {
        ClientOwnSupplement? ownSupplement = await _clientOwnSupplementRepository.GetTrackedByIdForUserWithMembersAsync(
            command.UserId,
            command.Id,
            cancellationToken);
        if (ownSupplement is null)
        {
            return Error.NotFound(
                description: "Own supplement not found.",
                metadata: new()
                {
                    ["resourceType"] = "ClientOwnSupplement",
                    ["resourceId"] = command.Id.ToString(),
                });
        }

        ErrorOr<(string Display, string Normalized)> normalizedName =
            ClientOwnSupplement.NormalizeName(command.Request.Name);
        if (normalizedName.IsError)
        {
            return normalizedName.Errors;
        }

        ClientOwnSupplement? existing = await _clientOwnSupplementRepository.FindByNormalizedNameForUserAsync(
            command.UserId,
            normalizedName.Value.Normalized,
            excludeId: command.Id,
            cancellationToken);
        if (existing is not null)
        {
            return Error.Conflict(
                description: "Own supplement with this name already exists.",
                metadata: new()
                {
                    ["field"] = "name",
                    ["conflictingResourceId"] = existing.Id.ToString(),
                });
        }

        List<Supplement> supplements = await _supplementRepository.GetByIdsAsync(
            command.Request.SupplementIds,
            cancellationToken);
        if (supplements.Count != command.Request.SupplementIds.Count)
        {
            Guid missingId = command.Request.SupplementIds
                .First(id => supplements.All(supplement => supplement.Id != id));

            return Error.NotFound(
                description: "Supplement not found.",
                metadata: new()
                {
                    ["resourceType"] = "Supplement",
                    ["resourceId"] = missingId.ToString(),
                });
        }

        ErrorOr<Success> updated = ownSupplement.Update(
            command.Request.Name,
            command.Request.SupplementIds);
        if (updated.IsError)
        {
            return updated.Errors;
        }

        ClientOwnSupplement? reloaded = await _clientOwnSupplementRepository.GetByIdForUserWithMembersAsync(
            command.UserId,
            command.Id,
            cancellationToken);

        return (reloaded ?? ownSupplement).MapToDetailsDto().MapToResponse();
    }
}
