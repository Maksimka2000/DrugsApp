using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Features.PatientSupervisionFeature.Shared;

namespace DrugsApp.Features.PatientSupervisionFeature.ListPatientHealthcareProfessionals;

internal static class ListPatientHealthcareProfessionalsMapping
{
    public static SearchPatientHealthcareProfessionalsQuery MapToQuery(this SearchPatientHealthcareProfessionalsRequest request, Guid patientUserId) =>
        new(
            PatientUserId: patientUserId,
            RelationshipState: request.RelationshipState,
            SortBy: request.SortBy,
            SortDirection: request.SortDirection,
            Skip: request.Skip,
            Take: request.Take);

    public static PatientHealthcareProfessionalListResponse MapToResponse(this SearchPatientHealthcareProfessionalsResult result, int requestedTake)
    {
        var items = result.Items.Select(row => new PatientHealthcareProfessionalListItemDto(
            row.EntryType,
            row.HealthcareProfessionalUserId,
            row.RequestId,
            row.ConnectionId,
            row.RelationshipState,
            row.AccessLevelCode,
            row.FirstName,
            row.LastName,
            row.ProfessionLabel,
            row.WorkplaceLabel,
            row.ShortDescription,
            row.HasPendingProposal,
            row.PendingProposalCount,
            row.RequestedAtUtc,
            row.ApprovedAtUtc,
            row.HasProfilePhoto)).ToList();

        bool hasMore = items.Count > requestedTake;
        var page = items.Take(requestedTake).ToList();

        return new PatientHealthcareProfessionalListResponse(
            Items: page,
            TotalCount: result.TotalCount,
            HasMore: hasMore);
    }
}
