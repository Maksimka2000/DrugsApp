using System;
using System.Collections.Generic;

namespace DrugsApp.Domain.AuthFeature.Interfaces;

public interface IJwtTokenProvider
{
    string GenerateAccessToken(
        Guid userId,
        string email,
        IReadOnlyCollection<string> roles);

    int GetAccessTokenLifetimeSeconds();

    int GetRefreshTokenLifetimeDays();
}
