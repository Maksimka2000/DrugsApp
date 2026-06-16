using System;
using System.Linq;
using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.Shared;
using DrugsApp.Features.PatientSupervisionFeature.Shared;

namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.ListProviderSupervisionRequests;

internal static class ListProviderSupervisionRequestsMapping
{
    public static ListProviderSupervisionRequestsQuery MapToQuery(
        this ListProviderSupervisionRequestsRequest request,
        Guid providerUserId) =>
        new(
            ProviderUserId: providerUserId,
            Ascending: request.OrderBy.Equals("ascending", StringComparison.OrdinalIgnoreCase),
            Skip: request.Skip,
            Take: request.Take);

    public static ProviderSupervisionRequestListResponse MapToResponse(
        this IReadOnlyCollection<ProviderSupervisionRequestListRow> rows,
        int requestedTake,
        int? totalCount)
    {
        var items = rows.Select(row => new ProviderSupervisionRequestListItemDto(
            RequestId: row.RequestId,
            PatientUserId: row.PatientUserId,
            RequestState: row.RequestState.ToString(),
            RequestedAtUtc: row.RequestedAtUtc,
            ExpiresAtUtc: row.ExpiresAtUtc,
            FirstName: row.FirstName,
            LastName: row.LastName,
            ProfilePhotoUrl: MemberProfilePhotoUrls.Build(
                row.PatientUserId,
                row.ProfilePhotoStorageKey))).ToList();

        bool hasMore = items.Count > requestedTake;
        var page = items.Take(requestedTake).ToList();

        return new ProviderSupervisionRequestListResponse(
            Items: page,
            TotalCount: totalCount,
            HasMore: hasMore);
    }
}
