using System;
using System.Linq;
using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Features.PatientSupervisionFeature.Shared;

namespace DrugsApp.Features.PatientSupervisionFeature.ListPatientSupervisionRequests;

internal static class ListPatientSupervisionRequestsMapping
{
    /// <summary>
    /// Builds a query for listing supervision requests for a patient.
    /// </summary>
    /// <param name="request">Incoming request payload.</param>
    /// <param name="patientUserId">Patient user identifier.</param>
    /// <returns>Query with normalized sort and paging.</returns>
    public static ListPatientSupervisionRequestsQuery MapToQuery(
        this ListPatientSupervisionRequestsRequest request,
        Guid patientUserId) =>
        new(
            PatientUserId: patientUserId,
            Ascending: request.OrderBy.Equals("ascending", StringComparison.OrdinalIgnoreCase),
            Skip: request.Skip,
            Take: request.Take);

    /// <summary>
    /// Maps supervision request rows to a response with paging metadata.
    /// </summary>
    /// <param name="rows">Fetched rows.</param>
    /// <param name="requestedTake">Requested page size.</param>
    /// <param name="totalCount">Optional total count.</param>
    /// <returns>Response DTO with items and paging info.</returns>
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
