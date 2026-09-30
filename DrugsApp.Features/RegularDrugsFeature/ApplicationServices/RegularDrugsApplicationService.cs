using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.DrugFeature.Repositories;
using DrugsApp.Domain.NotificationsFeature;
using DrugsApp.Domain.NotificationsFeature.Enums;
using DrugsApp.Domain.NotificationsFeature.Interfaces;
using DrugsApp.Domain.RegularDrugsFeature.Entities;
using DrugsApp.Domain.RegularDrugsFeature.Enums;
using DrugsApp.Domain.RegularDrugsFeature.Interfaces;
using DrugsApp.Domain.RegularDrugsFeature.Shared;
using DrugsApp.Features.RegularDrugsFeature.CreateRegularDrug;
using DrugsApp.Features.RegularDrugsFeature.Shared;
using DrugsApp.Features.RegularDrugsFeature.UpdateRegularDrug;
using DrugsApp.Features.Common.Limits;
using ErrorOr;

namespace DrugsApp.Features.RegularDrugsFeature.ApplicationServices;

/// <summary>
/// Application service coordinating regular drug schedule mutations and auto-expire workflows.
/// </summary>
public sealed class RegularDrugsApplicationService(
    IDrugRepository _drugRepository,
    IClientRegularDrugRepository _clientRegularDrugRepository,
    INotificationProjectionService _notificationProjectionService) : IRegularDrugsApplicationService
{
    private const int ExpireBatchSize = 50;

    /// <summary>
    /// Creates a regular drug schedule for the user.
    /// </summary>
    /// <param name="userId">Owner user identifier.</param>
    /// <param name="request">Creation payload.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Created regular drug DTO or errors.</returns>
    public async Task<ErrorOr<RegularDrugListItemDto>> CreateAsync(
        Guid userId,
        CreateRegularDrugRequest request,
        CancellationToken cancellationToken)
    {
        int existingCount = await _clientRegularDrugRepository.CountLiveByUserIdAsync(
            userId,
            drugNameQuery: null,
            startDate: null,
            cancellationToken);
        ErrorOr<Success> limitCheck = UserRecordLimits.EnsureBelowLimit(
            existingCount,
            UserRecordLimits.MAX_REGULAR_DRUGS,
            "RegularDrug");
        if (limitCheck.IsError)
        {
            return limitCheck.Errors;
        }

        Drug? drug = await _drugRepository.GetByIdAsync(request.DrugId, cancellationToken);
        if (drug is null)
        {
            return Error.NotFound(
                description: "Drug not found.",
                metadata: new()
                {
                    ["resourceType"] = "Drug",
                    ["resourceId"] = request.DrugId.ToString(),
                });
        }

        if (!RegularDrugEligibility.IsEligibleDrug(drug))
        {
            return Error.Validation(
                description: "Selected drug is not eligible.",
                metadata: new()
                {
                    ["field"] = "drugId",
                    ["reason"] = "eligibility",
                });
        }

        bool formExists = drug.DrugForms.Any(drugForm => drugForm.Id == request.DrugFormId);
        if (!formExists)
        {
            return Error.Validation(
                description: "Selected drug form does not belong to the drug.",
                metadata: new()
                {
                    ["field"] = "drugFormId",
                });
        }

        bool hasDuplicate = await _clientRegularDrugRepository.ExistsLiveForUserWithDrugIdAsync(
            userId,
            request.DrugId,
            excludeId: null,
            cancellationToken);
        if (hasDuplicate)
        {
            return Error.Conflict(
                description: "A live regular drug schedule already exists for this drug.",
                metadata: new()
                {
                    ["field"] = "drugId",
                    ["conflictingResourceId"] = request.DrugId.ToString(),
                });
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        ErrorOr<ClientRegularDrug> created = ClientRegularDrug.Create(
            userId,
            request.DrugId,
            request.DrugFormId,
            request.StartDate,
            request.EndDate,
            request.IntakeTimes,
            today);
        if (created.IsError)
        {
            return created.Errors;
        }

        DrugForm selectedForm = drug.DrugForms.First(drugForm => drugForm.Id == request.DrugFormId);
        created.Value.AttachCatalogReferences(drug, selectedForm);

        await _clientRegularDrugRepository.AddAsync(created.Value, cancellationToken);

        return created.Value.MapToListItemDto();
    }

    /// <summary>
    /// Updates an existing regular drug schedule for the user.
    /// </summary>
    /// <param name="userId">Owner user identifier.</param>
    /// <param name="id">Regular drug identifier.</param>
    /// <param name="request">Update payload.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Updated regular drug DTO or errors.</returns>
    public async Task<ErrorOr<RegularDrugListItemDto>> UpdateScheduleAsync(
        Guid userId,
        Guid id,
        UpdateRegularDrugRequest request,
        CancellationToken cancellationToken)
    {
        ClientRegularDrug? regularDrug = await _clientRegularDrugRepository.GetByIdForUserAsync(
            userId,
            id,
            track: true,
            cancellationToken);
        if (regularDrug is null)
        {
            return Error.NotFound(
                description: "Regular drug schedule not found.",
                metadata: new()
                {
                    ["resourceType"] = "ClientRegularDrug",
                    ["resourceId"] = id.ToString(),
                });
        }

        if (regularDrug.Status == RegularDrugStatus.Expired)
        {
            return Error.Conflict(
                description: "Regular drug schedule is archived and cannot be updated.",
                metadata: new()
                {
                    ["field"] = "status",
                    ["reason"] = "archived",
                });
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        ErrorOr<Success> updated = regularDrug.UpdateSchedule(
            request.StartDate,
            request.EndDate,
            request.IntakeTimes,
            today);
        if (updated.IsError)
        {
            return updated.Errors;
        }

        return regularDrug.MapToListItemDto();
    }

    /// <summary>
    /// Moves a live regular drug schedule to history with a reason.
    /// </summary>
    /// <param name="userId">Owner user identifier.</param>
    /// <param name="id">Regular drug identifier.</param>
    /// <param name="reason">History reason.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>History summary or errors.</returns>
    public async Task<ErrorOr<MoveRegularDrugToHistorySummaryDto>> MoveToHistoryAsync(
        Guid userId,
        Guid id,
        RegularDrugHistoryReason reason,
        CancellationToken cancellationToken)
    {
        ClientRegularDrug? regularDrug = await _clientRegularDrugRepository.GetByIdForUserAsync(
            userId,
            id,
            track: true,
            cancellationToken);
        if (regularDrug is null)
        {
            return Error.NotFound(
                description: "Regular drug schedule not found.",
                metadata: new()
                {
                    ["resourceType"] = "ClientRegularDrug",
                    ["resourceId"] = id.ToString(),
                });
        }

        if (regularDrug.Status == RegularDrugStatus.Expired)
        {
            return Error.Conflict(
                description: "Regular drug schedule is already archived.",
                metadata: new()
                {
                    ["field"] = "status",
                    ["reason"] = "archived",
                });
        }

        ErrorOr<Success> moved = regularDrug.MoveToHistory(reason, DateTime.UtcNow);
        if (moved.IsError)
        {
            return moved.Errors;
        }

        return regularDrug.MapToMoveToHistorySummaryDto();
    }

    /// <summary>
    /// Expires ended regular drug rows in batches and projects notifications.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Batch expire result.</returns>
    public async Task<ErrorOr<ExpireEndedRegularDrugsResult>> ExpireEndedRowsAsync(
        CancellationToken cancellationToken)
    {
        var asOfDate = DateOnly.FromDateTime(DateTime.UtcNow);

        List<ClientRegularDrug> candidates = await _clientRegularDrugRepository.FindEndedLiveRowsAsync(
            asOfDate,
            ExpireBatchSize,
            cancellationToken);

        if (candidates.Count == 0)
        {
            return new ExpireEndedRegularDrugsResult(ExpiredCount: 0, NotificationsCreated: 0);
        }

        int expiredCount = 0;
        int notificationsCreated = 0;

        foreach (ClientRegularDrug regularDrug in candidates)
        {
            ErrorOr<Success> moved = regularDrug.MoveToHistory(
                RegularDrugHistoryReason.AutoExpire,
                DateTime.UtcNow);
            if (moved.IsError)
            {
                return moved.Errors;
            }

            expiredCount++;

            NotificationProjectionDraft draft = new(
                UserId: regularDrug.UserId,
                SystemAnnouncementId: null,
                TypeCode: UserNotificationType.RegularDrugExpired,
                PrimaryTagCode: NotificationTag.MedicalData,
                AudienceScopeCode: NotificationAudienceScope.PatientOnly,
                Title: "Regular drug schedule ended",
                SummaryText: RegularDrugMappings.BuildNotificationSummary(regularDrug),
                BodyText: null,
                SourceEventKey: $"regular-drug-expired:{regularDrug.Id}",
                RelatedAggregateType: "client_regular_drug",
                RelatedAggregateId: regularDrug.Id,
                DeepLinkPath: "/regular-intakes/regular-drugs/history",
                RequiresAction: false,
                ActionState: NotificationActionState.Informational);

            ErrorOr<Success> projectionResult = await _notificationProjectionService.ProjectAsync(
                draft,
                cancellationToken);
            if (projectionResult.IsError)
            {
                return projectionResult.Errors;
            }

            notificationsCreated++;
        }

        return new ExpireEndedRegularDrugsResult(expiredCount, notificationsCreated);
    }
}
