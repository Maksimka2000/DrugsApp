using System.Threading;
using System.Threading.Tasks;
using ErrorOr;

namespace DrugsApp.Domain.AuthFeature.Interfaces;

/// <summary>
/// Protects the configured bootstrap super-admin account from destructive admin mutations.
/// </summary>
public interface ISuperAdminGuard
{
    /// <summary>
    /// Returns true when the email matches the configured super-admin bootstrap account.
    /// </summary>
    bool IsProtectedEmail(string email);

    /// <summary>
    /// Rejects mutations against the protected bootstrap account.
    /// </summary>
    Task<ErrorOr<Success>> EnsureMutableAsync(
        string email,
        CancellationToken cancellationToken);
}
