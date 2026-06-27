using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature.Entities;

namespace DrugsApp.Domain.AuthFeature.Interfaces;

/// <summary>
/// Persistence contract for refresh tokens.
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>
    /// Stores a new refresh token instance.
    /// </summary>
    /// <param name="refreshToken">Token entity to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a refresh token by its string value.
    /// </summary>
    /// <param name="token">Token value to search for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching token or null when not found.</returns>
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes all refresh tokens belonging to a user.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
