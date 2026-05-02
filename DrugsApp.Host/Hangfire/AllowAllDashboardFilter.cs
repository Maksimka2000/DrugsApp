using Hangfire.Dashboard;

namespace DrugsApp.Host.Hangfire;

/// <summary>
/// Permissive Hangfire dashboard authorization filter for local/dev use.
/// </summary>
public sealed class AllowAllDashboardFilter : IDashboardAuthorizationFilter
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public bool Authorize(DashboardContext context) => true;
}
