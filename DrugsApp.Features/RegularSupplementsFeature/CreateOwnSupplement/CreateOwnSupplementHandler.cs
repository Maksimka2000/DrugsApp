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

namespace DrugsApp.Features.RegularSupplementsFeature.CreateOwnSupplement;

/// <summary>
/// Handles creation of own supplement compounds.
/// </summary>
internal sealed class CreateOwnSupplementHandler(
    ISupplementRepository _supplementRepository,
    IClientOwnSupplementRepository _clientOwnSupplementRepository)
    : IRequestHandler<CreateOwnSupplementCommand, ErrorOr<CreateOwnSupplementResponse>>
{
    private const int MaxOwnSupplementsPerUser = 50;

    public async Task<ErrorOr<CreateOwnSupplementResponse>> Handle(
        CreateOwnSupplementCommand command,
        CancellationToken cancellationToken)
    {
        int existingCount = await _clientOwnSupplementRepository.CountByUserIdAsync(
            command.UserId,
            cancellationToken);
        if (existingCount >= MaxOwnSupplementsPerUser)
        {
            return Error.Conflict(
                description: "Own supplement limit reached.",
                metadata: new()
                {
                    ["field"] = "limit",
                    ["maxCount"] = MaxOwnSupplementsPerUser,
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
            excludeId: null,
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

        ErrorOr<ClientOwnSupplement> created = ClientOwnSupplement.Create(
            command.UserId,
            command.Request.Name,
            command.Request.SupplementIds);
        if (created.IsError)
        {
            return created.Errors;
        }

        await _clientOwnSupplementRepository.AddAsync(created.Value, cancellationToken);

        ClientOwnSupplement? reloaded = await _clientOwnSupplementRepository.GetByIdForUserWithMembersAsync(
            command.UserId,
            created.Value.Id,
            cancellationToken);

        return (reloaded ?? created.Value).MapToDetailsDto().MapToResponse();
    }
}
