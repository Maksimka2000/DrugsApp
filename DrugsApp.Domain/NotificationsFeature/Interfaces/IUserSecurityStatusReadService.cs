using System;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;

namespace DrugsApp.Domain.NotificationsFeature.Interfaces;

/// <summary>
/// Thin read wrapper over identity security flags for notification hydration.
/// </summary>
public interface IUserSecurityStatusReadService
{
    /// <summary>
    /// Retrieves the current security status flags for the specified user.
    /// </summary>
    /// <param name="userId">User whose security status should be returned.</param>
    /// <param name="cancellationToken">Propagation token to cancel the query.</param>
    /// <returns>Security snapshot describing email confirmation and MFA flags.</returns>
    Task<ErrorOr<UserSecurityStatusSnapshot>> GetStatusAsync(
        Guid userId,
        CancellationToken cancellationToken);
}

/// <summary>
/// Security status snapshot for notification detail hydration.
/// </summary>
public sealed record UserSecurityStatusSnapshot(
    bool EmailConfirmed,
    bool TwoFactorEnabled);
