using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Features.PatientSupervisionFeature.Shared;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.PatientSupervisionFeature.ListPatientSupervisionRequests;

/// <summary>
/// Request payload for listing patient supervision requests.
/// </summary>
/// <param name="OrderBy">Order direction (ascending/descending).</param>
/// <param name="Skip">Number of records to skip.</param>
/// <param name="Take">Number of records to take.</param>
public sealed record ListPatientSupervisionRequestsRequest(
    [FromQuery(Name = "order_by")] string OrderBy = "descending",
    [FromQuery(Name = "skip")] int Skip = 0,
    [FromQuery(Name = "take")] int Take = 20);

/// <summary>
/// Query for listing patient supervision requests.
/// </summary>
/// <param name="PatientUserId">Identifier of the patient.</param>
/// <param name="Ascending">Sort direction flag.</param>
/// <param name="Skip">Number of records to skip.</param>
/// <param name="Take">Number of records to take.</param>
public sealed record ListPatientSupervisionRequestsQuery(
    Guid PatientUserId,
    bool Ascending,
    int Skip,
    int Take) : IRequest<ErrorOr<PatientSupervisionRequestListResponse>>;

internal sealed class ListPatientSupervisionRequestsHandler(ISupervisionReadRepository supervisionReadRepository)
    : IRequestHandler<ListPatientSupervisionRequestsQuery, ErrorOr<PatientSupervisionRequestListResponse>>
{
    public async Task<ErrorOr<PatientSupervisionRequestListResponse>> Handle(
        ListPatientSupervisionRequestsQuery query,
        CancellationToken cancellationToken)
    {
        int takePlusOne = query.Take + 1;

        IReadOnlyCollection<PatientSupervisionRequestListRow> rows =
            await supervisionReadRepository.ListPatientSupervisionRequestsAsync(
                query.PatientUserId,
                query.Ascending,
                query.Skip,
                takePlusOne,
                cancellationToken);

        int? totalCount = null;
        if (query.Skip == 0)
        {
            totalCount = await supervisionReadRepository.CountPatientSupervisionRequestsAsync(
                query.PatientUserId,
                cancellationToken);
        }

        return rows.MapToResponse(query.Take, totalCount);
    }
}
