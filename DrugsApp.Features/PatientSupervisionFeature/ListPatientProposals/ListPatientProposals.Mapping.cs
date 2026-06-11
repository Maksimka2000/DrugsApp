using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Features.PatientSupervisionFeature.Shared;

namespace DrugsApp.Features.PatientSupervisionFeature.ListPatientProposals;

internal static class ListPatientProposalsMapping
{
    public static ListPatientProposalsQuery MapToQuery(this ListPatientProposalsRequest request, Guid patientUserId) =>
        new(
            PatientUserId: patientUserId,
            HealthcareProfessionalUserId: request.HealthcareProfessionalUserId,
            ProposalState: request.ProposalState,
            Skip: request.Skip,
            Take: request.Take);

    public static PatientProposalListResponse MapToResponse(this IReadOnlyCollection<MedicalChangeProposalListRow> rows, int requestedTake)
    {
        var items = rows.Select(r => new PatientProposalListItemDto(
            r.ProposalId,
            r.HealthcareProfessionalUserId,
            r.ProposalState,
            r.ProposalSummary,
            r.SubmittedAtUtc)).ToList();

        bool hasMore = items.Count > requestedTake;
        var page = items.Take(requestedTake).ToList();

        return new PatientProposalListResponse(page, TotalCount: rows.Count, HasMore: hasMore);
    }
}
