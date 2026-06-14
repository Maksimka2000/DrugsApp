using DrugsApp.Host.Auth.Jwt;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace DrugsApp.Host.Hangfire;

/// <summary>
/// Persists the Hangfire dashboard JWT from the query string into a scoped cookie so
/// subsequent CSS/JS asset requests can authenticate without repeating access_token.
/// </summary>
public sealed class HangfireDashboardAccessTokenMiddleware
{
    /// <summary>
    /// Cookie name used for Hangfire dashboard JWT propagation.
    /// </summary>
    public const string CookieName = "hangfire_access_token";

    private readonly RequestDelegate _next;
    private readonly HangfireSettings _hangfireSettings;
    private readonly JwtSettings _jwtSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="HangfireDashboardAccessTokenMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware delegate in the pipeline.</param>
    /// <param name="hangfireSettings">Hangfire host settings.</param>
    /// <param name="jwtSettings">JWT settings used for cookie lifetime.</param>
    public HangfireDashboardAccessTokenMiddleware(
        RequestDelegate next,
        HangfireSettings hangfireSettings,
        IOptions<JwtSettings> jwtSettings)
    {
        _next = next;
        _hangfireSettings = hangfireSettings;
        _jwtSettings = jwtSettings.Value;
    }

    /// <summary>
    /// Stores the dashboard access token in a HttpOnly cookie when it is supplied via query string.
    /// </summary>
    /// <param name="context">Current HTTP context.</param>
    /// <returns>Task that completes when the request pipeline finishes.</returns>
    public Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments(_hangfireSettings.DashboardPath))
        {
            string? accessToken = context.Request.Query["access_token"];
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                context.Response.Cookies.Append(
                    CookieName,
                    accessToken,
                    new CookieOptions
                    {
                        HttpOnly = true,
                        SameSite = SameSiteMode.Lax,
                        Path = _hangfireSettings.DashboardPath,
                        Secure = context.Request.IsHttps,
                        MaxAge = TimeSpan.FromMinutes(_jwtSettings.AccessTokenMinutes)
                    });
            }
        }

        return _next(context);
    }
}
