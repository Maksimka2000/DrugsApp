using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Features.PatientSupervisionFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.PatientSupervisionFeature.ListPatientHealthcareProfessionals;

public sealed record SearchPatientHealthcareProfessionalsRequest(
    string? RelationshipState = "all",
    string? SortBy = "status",
    string? SortDirection = "asc",
    int Skip = 0,
    int Take = 20);

public sealed record SearchPatientHealthcareProfessionalsQuery(
    Guid PatientUserId,
    string? RelationshipState,
    string? SortBy,
    string? SortDirection,
    int Skip,
    int Take) : IRequest<ErrorOr<PatientHealthcareProfessionalListResponse>>;

internal sealed class SearchPatientHealthcareProfessionalsHandler(ISupervisionReadRepository supervisionReadRepository)
    : IRequestHandler<SearchPatientHealthcareProfessionalsQuery, ErrorOr<PatientHealthcareProfessionalListResponse>>
{
    public async Task<ErrorOr<PatientHealthcareProfessionalListResponse>> Handle(
        SearchPatientHealthcareProfessionalsQuery query,
        CancellationToken cancellationToken)
    {
        SearchPatientHealthcareProfessionalsResult result = await supervisionReadRepository.SearchPatientHealthcareProfessionalsAsync(
            query.PatientUserId,
            query.RelationshipState,
            query.SortBy,
            query.SortDirection?.Equals("asc", StringComparison.OrdinalIgnoreCase) ?? true,
            query.Skip,
            query.Take,
            cancellationToken);

        return result.MapToResponse(query.Take);
    }
}
