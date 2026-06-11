namespace DrugsApp.Features.AdminUserManagementFeature.ListAdminUsers;

internal static class ListAdminUsersMapping
{
    public static ListAdminUsersQuery MapToQuery(this ListAdminUsersRequest request, Guid currentUserId) =>
        new(
            Skip: request.Skip,
            Take: request.Take,
            Search: request.Search,
            CurrentUserId: currentUserId);
}
