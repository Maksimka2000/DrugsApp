using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.RegularSupplementsFeature.Entities;
using DrugsApp.Domain.RegularSupplementsFeature.Enums;
using DrugsApp.Domain.RegularSupplementsFeature.Repositories;
using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Domain.SupplementFeature.Repositories;
using DrugsApp.Features.RegularSupplementsFeature.Shared;
using DrugsApp.Features.Common.Limits;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.RegularSupplementsFeature.CreateRegularSupplement;

/// <summary>
/// Handles creation of regular supplement schedules.
/// </summary>
internal sealed class CreateRegularSupplementHandler(
    ISupplementRepository _supplementRepository,
    IClientOwnSupplementRepository _clientOwnSupplementRepository,
    IClientRegularSupplementRepository _clientRegularSupplementRepository)
    : IRequestHandler<CreateRegularSupplementCommand, ErrorOr<CreateRegularSupplementResponse>>
{
    public async Task<ErrorOr<CreateRegularSupplementResponse>> Handle(
        CreateRegularSupplementCommand command,
        CancellationToken cancellationToken)
    {
        int existingCount = await _clientRegularSupplementRepository.CountLiveByUserIdAsync(
            command.UserId,
            nameQuery: null,
            startDate: null,
            cancellationToken);
        ErrorOr<Success> limitCheck = UserRecordLimits.EnsureBelowLimit(
            existingCount,
            UserRecordLimits.MAX_REGULAR_SUPPLEMENTS,
            "RegularSupplement");
        if (limitCheck.IsError)
        {
            return limitCheck.Errors;
        }

        RegularSupplementSource? source = RegularSupplementMappings.MapSourceFromToken(command.Request.Source);
        if (source is null)
        {
            return Error.Validation(
                description: "Invalid source value.",
                metadata: new()
                {
                    ["field"] = "source",
                });
        }

        Guid? supplementId = null;
        Guid? ownSupplementId = null;

        if (source == RegularSupplementSource.Database)
        {
            supplementId = command.Request.SupplementId!.Value;

            Supplement? supplement = await _supplementRepository.GetByIdAsync(supplementId.Value, cancellationToken);
            if (supplement is null)
            {
                return Error.NotFound(
                    description: "Supplement not found.",
                    metadata: new()
                    {
                        ["resourceType"] = "Supplement",
                        ["resourceId"] = supplementId.Value.ToString(),
                    });
            }
        }
        else
        {
            ownSupplementId = command.Request.OwnSupplementId!.Value;

            ClientOwnSupplement? ownSupplement = await _clientOwnSupplementRepository.GetByIdForUserWithMembersAsync(
                command.UserId,
                ownSupplementId.Value,
                cancellationToken);
            if (ownSupplement is null)
            {
                return Error.NotFound(
                    description: "Own supplement not found.",
                    metadata: new()
                    {
                        ["resourceType"] = "ClientOwnSupplement",
                        ["resourceId"] = ownSupplementId.Value.ToString(),
                    });
            }
        }

        bool hasDuplicate = await _clientRegularSupplementRepository.ExistsLiveDuplicateForUserAsync(
            command.UserId,
            source.Value,
            supplementId,
            ownSupplementId,
            excludeId: null,
            cancellationToken);
        if (hasDuplicate)
        {
            return Error.Conflict(
                description: "A live regular supplement schedule already exists for this target.",
                metadata: new()
                {
                    ["field"] = source == RegularSupplementSource.Database ? "supplementId" : "ownSupplementId",
                });
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        ErrorOr<ClientRegularSupplement> created = ClientRegularSupplement.Create(
            command.UserId,
            source.Value,
            supplementId,
            ownSupplementId,
            command.Request.StartDate,
            command.Request.EndDate,
            command.Request.IntakeTimes,
            today);
        if (created.IsError)
        {
            return created.Errors;
        }

        if (source == RegularSupplementSource.Database)
        {
            Supplement supplement = (await _supplementRepository.GetByIdAsync(supplementId!.Value, cancellationToken))!;
            created.Value.AttachCatalogReference(supplement);
        }
        else
        {
            ClientOwnSupplement ownSupplement = (await _clientOwnSupplementRepository.GetByIdForUserWithMembersAsync(
                command.UserId,
                ownSupplementId!.Value,
                cancellationToken))!;
            created.Value.AttachOwnSupplementReference(ownSupplement);
        }

        await _clientRegularSupplementRepository.AddAsync(created.Value, cancellationToken);

        return created.Value.MapToDetailsDto().MapToResponse();
    }
}
