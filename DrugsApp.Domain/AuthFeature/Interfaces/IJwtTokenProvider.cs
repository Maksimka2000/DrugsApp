using System;
using System.Collections.Generic;

namespace DrugsApp.Domain.AuthFeature.Interfaces;

/// <summary>
/// Issues JWT access tokens and exposes configured lifetimes.
/// </summary>
public interface IJwtTokenProvider
{
    /// <summary>
    /// Generates a signed JWT for the given user and roles.
    /// </summary>
    /// <param name="userId">User identifier to embed.</param>
    /// <param name="email">User email to embed.</param>
    /// <param name="roles">Role claims to include.</param>
    /// <returns>Signed JWT access token.</returns>
    string GenerateAccessToken(
        Guid userId,
        string email,
        IReadOnlyCollection<string> roles);

    /// <summary>
    /// Gets the configured access token lifetime in seconds.
    /// </summary>
    /// <returns>Access token validity window in seconds.</returns>
    int GetAccessTokenLifetimeSeconds();

    /// <summary>
    /// Gets the configured refresh token lifetime in days.
    /// </summary>
    /// <returns>Refresh token validity window in days.</returns>
    int GetRefreshTokenLifetimeDays();
}
