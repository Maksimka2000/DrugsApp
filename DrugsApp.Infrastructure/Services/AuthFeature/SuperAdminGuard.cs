using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature.Interfaces;
using ErrorOr;
using Microsoft.Extensions.Options;

namespace DrugsApp.Infrastructure.Services.AuthFeature;

/// <summary>
/// Matches protected bootstrap accounts against configured super-admin email.
/// </summary>
public sealed class SuperAdminGuard(IOptions<SuperAdminSettings> superAdminOptions) : ISuperAdminGuard
{
    private readonly SuperAdminSettings _superAdminSettings = superAdminOptions.Value;

    /// <summary>
    /// Checks whether the provided email matches the protected super-admin account.
    /// </summary>
    /// <param name="email">Email to compare.</param>
    /// <returns><c>true</c> when the email is protected.</returns>
    public bool IsProtectedEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(_superAdminSettings.Email))
        {
            return false;
        }

        return string.Equals(
            email.Trim(),
            _superAdminSettings.Email.Trim(),
            StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Ensures the target email is not protected; returns forbidden error if it is.
    /// </summary>
    /// <param name="email">Email to validate.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Success or forbidden error.</returns>
    public Task<ErrorOr<Success>> EnsureMutableAsync(
        string email,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (IsProtectedEmail(email))
        {
            return Task.FromResult<ErrorOr<Success>>(Error.Forbidden(
                code: "Auth.ProtectedAccount",
                description: "This account is protected and cannot be modified."));
        }

        return Task.FromResult<ErrorOr<Success>>(Result.Success);
    }
}
