using System;
using System.Linq;
using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Features.PatientSupervisionFeature.Shared;

namespace DrugsApp.Features.PatientSupervisionFeature.ListPatientSupervisionRequests;

internal static class ListPatientSupervisionRequestsMapping
{
    public static ListPatientSupervisionRequestsQuery MapToQuery(
        this ListPatientSupervisionRequestsRequest request,
        Guid patientUserId) =>
        new(
            PatientUserId: patientUserId,
            Ascending: request.OrderBy.Equals("ascending", StringComparison.OrdinalIgnoreCase),
            Skip: request.Skip,
            Take: request.Take);

    public static PatientSupervisionRequestListResponse MapToResponse(
        this IReadOnlyCollection<PatientSupervisionRequestListRow> rows,
        int requestedTake,
        int? totalCount)
    {
        var items = rows.Select(row => new PatientSupervisionRequestListItemDto(
            RequestId: row.RequestId,
            HealthcareProfessionalUserId: row.HealthcareProfessionalUserId,
            RequestState: row.RequestState.ToString(),
            RequestedAtUtc: row.RequestedAtUtc,
            FirstName: row.FirstName,
            LastName: row.LastName,
            ProfessionLabel: row.ProfessionLabel,
            WorkplaceLabel: row.WorkplaceLabel,
            ShortDescription: row.ShortDescription,
            ProfilePhotoUrl: MemberProfilePhotoUrls.Build(
                row.HealthcareProfessionalUserId,
                row.ProfilePhotoStorageKey))).ToList();

        bool hasMore = items.Count > requestedTake;
        var page = items.Take(requestedTake).ToList();

        return new PatientSupervisionRequestListResponse(
            Items: page,
            TotalCount: totalCount,
            HasMore: hasMore);
    }
}
