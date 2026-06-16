using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Repositories;
using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.Shared;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.ListProviderSupervisionRequests;

public sealed record ListProviderSupervisionRequestsRequest(
    [FromQuery(Name = "order_by")] string OrderBy = "descending",
    [FromQuery(Name = "skip")] int Skip = 0,
    [FromQuery(Name = "take")] int Take = 20);

public sealed record ListProviderSupervisionRequestsQuery(
    Guid ProviderUserId,
    bool Ascending,
    int Skip,
    int Take) : IRequest<ErrorOr<ProviderSupervisionRequestListResponse>>;

internal sealed class ListProviderSupervisionRequestsHandler(
    ISupervisionReadRepository supervisionReadRepository,
    IHealthcareProviderProfileRepository healthcareProviderProfileRepository)
    : IRequestHandler<ListProviderSupervisionRequestsQuery, ErrorOr<ProviderSupervisionRequestListResponse>>
{
    public async Task<ErrorOr<ProviderSupervisionRequestListResponse>> Handle(
        ListProviderSupervisionRequestsQuery query,
        CancellationToken cancellationToken)
    {
        HealthcareProviderProfile? profile = await healthcareProviderProfileRepository.GetTrackedByUserIdAsync(
            query.ProviderUserId,
            cancellationToken);

        if (profile is null)
        {
            return Error.Forbidden(description: "Provider profile required.");
        }

        int takePlusOne = query.Take + 1;

        IReadOnlyCollection<ProviderSupervisionRequestListRow> rows =
            await supervisionReadRepository.ListProviderSupervisionRequestsAsync(
                query.ProviderUserId,
                query.Ascending,
                query.Skip,
                takePlusOne,
                cancellationToken);

        int? totalCount = null;
        if (query.Skip == 0)
        {
            totalCount = await supervisionReadRepository.CountProviderSupervisionRequestsAsync(
                query.ProviderUserId,
                cancellationToken);
        }

        return rows.MapToResponse(query.Take, totalCount);
    }
}
