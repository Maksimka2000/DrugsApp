using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Features.PatientSupervisionFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.PatientSupervisionFeature.ListPatientProposals;

/// <summary>
/// Request payload for listing patient proposals.
/// </summary>
/// <param name="HealthcareProfessionalUserId">Optional healthcare professional filter.</param>
/// <param name="ProposalState">Optional proposal state filter.</param>
/// <param name="Skip">Number of records to skip.</param>
/// <param name="Take">Number of records to take.</param>
public sealed record ListPatientProposalsRequest(
    Guid? HealthcareProfessionalUserId = null,
    string? ProposalState = "pending",
    int Skip = 0,
    int Take = 20);

/// <summary>
/// Query for listing patient proposals.
/// </summary>
/// <param name="PatientUserId">Patient user identifier.</param>
/// <param name="HealthcareProfessionalUserId">Optional healthcare professional filter.</param>
/// <param name="ProposalState">Proposal state filter.</param>
/// <param name="Skip">Number of records to skip.</param>
/// <param name="Take">Number of records to take.</param>
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
