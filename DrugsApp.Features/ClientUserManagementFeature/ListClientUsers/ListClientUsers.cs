using DrugsApp.Features.ClientUserManagementFeature.ApplicationServices;
using DrugsApp.Features.ClientUserManagementFeature.Shared;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.ClientUserManagementFeature.ListClientUsers;

/// <summary>
/// Request payload for listing client users.
/// </summary>
/// <param name="Skip">Number of records to skip.</param>
/// <param name="Take">Number of records to take.</param>
/// <param name="Search">Optional search text.</param>
public sealed record ListClientUsersRequest(
    [FromQuery(Name = "skip")] int Skip = 0,
    [FromQuery(Name = "take")] int Take = 20,
    [FromQuery(Name = "search")] string? Search = null);

/// <summary>
/// Query for listing client users.
/// </summary>
/// <param name="Skip">Number of records to skip.</param>
/// <param name="Take">Number of records to take.</param>
/// <param name="Search">Optional search text.</param>
public sealed record ListClientUsersQuery(
    int Skip,
    int Take,
    string? Search) : IRequest<ErrorOr<ListClientUsersResponse>>;

internal sealed class ListClientUsersHandler(IClientUserManagementApplicationService clientUserManagementApplicationService)
    : IRequestHandler<ListClientUsersQuery, ErrorOr<ListClientUsersResponse>>
{
    public Task<ErrorOr<ListClientUsersResponse>> Handle(
        ListClientUsersQuery query,
        CancellationToken cancellationToken) =>
        clientUserManagementApplicationService.ListAsync(
            query.Skip,
            query.Take,
            query.Search,
            cancellationToken);
}
