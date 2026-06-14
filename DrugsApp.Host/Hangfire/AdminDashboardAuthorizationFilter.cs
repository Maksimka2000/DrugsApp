using System.Linq;
using DrugsApp.Domain.AuthFeature;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;

namespace DrugsApp.Host.Hangfire;

/// <summary>
/// Restricts Hangfire dashboard access to authenticated admin-portal roles.
/// </summary>
public sealed class AdminDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    /// <inheritdoc />
    public bool Authorize(DashboardContext context)
    {
        HttpContext httpContext = context.GetHttpContext();

        return httpContext.User.Identity?.IsAuthenticated == true
            && AuthRoles.AdminRoleNames.Any(httpContext.User.IsInRole);
    }
}
