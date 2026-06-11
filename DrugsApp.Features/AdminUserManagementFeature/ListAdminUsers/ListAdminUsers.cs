using DrugsApp.Features.AdminUserManagementFeature.ApplicationServices;
using DrugsApp.Features.AdminUserManagementFeature.Shared;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.AdminUserManagementFeature.ListAdminUsers;

public sealed record ListAdminUsersRequest(
    [FromQuery(Name = "skip")] int Skip = 0,
    [FromQuery(Name = "take")] int Take = 20,
    [FromQuery(Name = "search")] string? Search = null);

public sealed record ListAdminUsersQuery(
    int Skip,
    int Take,
    string? Search,
    Guid CurrentUserId) : IRequest<ErrorOr<ListAdminUsersResponse>>;

internal sealed class ListAdminUsersHandler(IAdminUserManagementApplicationService adminUserManagementApplicationService)
    : IRequestHandler<ListAdminUsersQuery, ErrorOr<ListAdminUsersResponse>>
{
    public Task<ErrorOr<ListAdminUsersResponse>> Handle(
        ListAdminUsersQuery query,
        CancellationToken cancellationToken) =>
        adminUserManagementApplicationService.ListAsync(
            query.Skip,
            query.Take,
            query.Search,
            query.CurrentUserId,
            cancellationToken);
}
