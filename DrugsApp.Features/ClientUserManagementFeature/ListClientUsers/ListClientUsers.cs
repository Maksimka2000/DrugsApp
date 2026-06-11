using DrugsApp.Features.ClientUserManagementFeature.ApplicationServices;
using DrugsApp.Features.ClientUserManagementFeature.Shared;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.ClientUserManagementFeature.ListClientUsers;

public sealed record ListClientUsersRequest(
    [FromQuery(Name = "skip")] int Skip = 0,
    [FromQuery(Name = "take")] int Take = 20,
    [FromQuery(Name = "search")] string? Search = null);

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
