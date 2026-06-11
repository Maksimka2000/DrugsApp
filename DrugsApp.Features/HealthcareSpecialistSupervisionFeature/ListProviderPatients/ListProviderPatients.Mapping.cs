using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.Shared;

namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.ListProviderPatients;

internal static class ListProviderPatientsMapping
{
    public static ListProviderPatientsQuery MapToQuery(this ListProviderPatientsRequest request, Guid providerUserId) =>
        new(
            ProviderUserId: providerUserId,
            RelationshipState: request.RelationshipState,
            SortBy: request.SortBy,
            SortDirection: request.SortDirection,
            Skip: request.Skip,
            Take: request.Take);

    public static ProviderPatientListResponse MapToResponse(this SearchProviderPatientsResult result, int requestedTake)
    {
        var items = result.Items.Select(r => new ProviderPatientListItemDto(
            r.EntryType,
            r.PatientUserId,
            r.RequestId,
            r.ConnectionId,
            r.RelationshipState,
            r.AccessLevelCode,
            r.FirstName,
            r.LastName,
            r.HasProfilePhoto,
            r.RequestedAtUtc,
            r.ApprovedAtUtc)).ToList();

        bool hasMore = items.Count > requestedTake;
        var page = items.Take(requestedTake).ToList();

        return new ProviderPatientListResponse(
            Items: page,
            TotalCount: result.TotalCount,
            HasMore: hasMore);
    }
}
