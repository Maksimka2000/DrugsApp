using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Features.PatientSupervisionFeature.Shared;

namespace DrugsApp.Features.PatientSupervisionFeature.ListPatientProposals;

internal static class ListPatientProposalsMapping
{
    /// <summary>
    /// Builds a query for listing patient proposals.
    /// </summary>
    /// <param name="request">Incoming request payload.</param>
    /// <param name="patientUserId">Patient user identifier.</param>
    /// <returns>Query populated with filters and paging.</returns>
    public static ListPatientProposalsQuery MapToQuery(this ListPatientProposalsRequest request, Guid patientUserId) =>
        new(
            PatientUserId: patientUserId,
            HealthcareProfessionalUserId: request.HealthcareProfessionalUserId,
            ProposalState: request.ProposalState,
            Skip: request.Skip,
            Take: request.Take);

    /// <summary>
    /// Maps proposal list rows to a response with paging metadata.
    /// </summary>
    /// <param name="rows">Rows returned from repository.</param>
    /// <param name="requestedTake">Requested page size.</param>
    /// <returns>Response DTO with items and paging info.</returns>
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
