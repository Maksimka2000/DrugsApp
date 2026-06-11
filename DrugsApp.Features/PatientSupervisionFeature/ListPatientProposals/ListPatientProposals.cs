using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Features.PatientSupervisionFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.PatientSupervisionFeature.ListPatientProposals;

public sealed record ListPatientProposalsRequest(
    Guid? HealthcareProfessionalUserId = null,
    string? ProposalState = "pending",
    int Skip = 0,
    int Take = 20);

public sealed record ListPatientProposalsQuery(
    Guid PatientUserId,
    Guid? HealthcareProfessionalUserId,
    string? ProposalState,
    int Skip,
    int Take) : IRequest<ErrorOr<PatientProposalListResponse>>;

internal sealed class ListPatientProposalsHandler(IMedicalChangeProposalRepository proposalRepository)
    : IRequestHandler<ListPatientProposalsQuery, ErrorOr<PatientProposalListResponse>>
{
    public async Task<ErrorOr<PatientProposalListResponse>> Handle(
        ListPatientProposalsQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<MedicalChangeProposalListRow> rows = await proposalRepository.SearchForPatientAsync(
            query.PatientUserId,
            query.HealthcareProfessionalUserId,
            query.ProposalState,
            query.Skip,
            query.Take,
            cancellationToken);

        return rows.MapToResponse(query.Take);
    }
}
