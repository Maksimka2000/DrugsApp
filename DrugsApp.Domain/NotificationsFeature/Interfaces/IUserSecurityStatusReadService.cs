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
