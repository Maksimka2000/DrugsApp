using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Repositories;
using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.ListProviderPatients;

public sealed record ListProviderPatientsRequest(
    string? RelationshipState = "all",
    string? SortBy = "status",
    string? SortDirection = "asc",
    int Skip = 0,
    int Take = 20);

public sealed record ListProviderPatientsQuery(
    Guid ProviderUserId,
    string? RelationshipState,
    string? SortBy,
    string? SortDirection,
    int Skip,
    int Take) : IRequest<ErrorOr<ProviderPatientListResponse>>;

internal sealed class ListProviderPatientsHandler(
    ISupervisionReadRepository supervisionReadRepository,
    IHealthcareProviderProfileRepository healthcareProviderProfileRepository)
    : IRequestHandler<ListProviderPatientsQuery, ErrorOr<ProviderPatientListResponse>>
{
    public async Task<ErrorOr<ProviderPatientListResponse>> Handle(
        ListProviderPatientsQuery query,
        CancellationToken cancellationToken)
    {
        HealthcareProviderProfile? profile = await healthcareProviderProfileRepository.GetTrackedByUserIdAsync(
            query.ProviderUserId,
            cancellationToken);

        if (profile is null)
        {
            return Error.Forbidden(description: "Provider profile required.");
        }

        SearchProviderPatientsResult result = await supervisionReadRepository.SearchProviderPatientsAsync(
            query.ProviderUserId,
            query.RelationshipState,
            query.SortBy,
            query.SortDirection?.Equals("asc", StringComparison.OrdinalIgnoreCase) ?? true,
            query.Skip,
            query.Take,
            cancellationToken);

        return result.MapToResponse(query.Take);
    }
}
