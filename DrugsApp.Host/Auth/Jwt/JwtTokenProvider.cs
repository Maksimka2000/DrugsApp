using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using DrugsApp.Domain.AuthFeature.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DrugsApp.Host.Auth.Jwt;

/// <summary>
/// Issues JWT access tokens using configured signing settings.
/// </summary>
public sealed class JwtTokenProvider(IOptions<JwtSettings> jwtOptions) : IJwtTokenProvider
{
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;

    /// <summary>
    /// Generates an access token for the given user and roles.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="email">User email.</param>
    /// <param name="roles">Roles to embed.</param>
    /// <returns>Serialized JWT string.</returns>
    public string GenerateAccessToken(
        Guid userId,
        string email,
        IReadOnlyCollection<string> roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenMinutes),
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Returns access token lifetime in seconds.
    /// </summary>
    public int GetAccessTokenLifetimeSeconds() => _jwtSettings.AccessTokenMinutes * 60;

    /// <summary>
    /// Returns refresh token lifetime in days.
    /// </summary>
    public int GetRefreshTokenLifetimeDays() => _jwtSettings.RefreshTokenDays;
}
