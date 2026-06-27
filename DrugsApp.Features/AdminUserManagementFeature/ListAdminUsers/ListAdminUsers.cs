using DrugsApp.Features.AdminUserManagementFeature.ApplicationServices;
using DrugsApp.Features.AdminUserManagementFeature.Shared;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.AdminUserManagementFeature.ListAdminUsers;

/// <summary>
/// Request payload for listing admin users with optional search filter.
/// </summary>
public sealed record ListAdminUsersRequest(
    [FromQuery(Name = "skip")] int Skip = 0,
    [FromQuery(Name = "take")] int Take = 20,
    [FromQuery(Name = "search")] string? Search = null);

/// <summary>
/// Query to list admin users with paging and search context.
/// </summary>
/// <param name="Skip">Number of items to skip.</param>
/// <param name="Take">Maximum items to return.</param>
/// <param name="Search">Optional free-text filter.</param>
/// <param name="CurrentUserId">Identifier of the requesting admin.</param>
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
