using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AccountFeature.Interfaces;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Domain.AuthFeature.Interfaces;
using ErrorOr;

namespace DrugsApp.Features.AccountFeature.ApplicationServices;

/// <summary>
/// Admin-facing service that validates a client account and performs a full teardown.
/// </summary>
public sealed class ClientAccountAdminDeletionService(
    IUserRoleReadService _userRoleReadService,
    IIdentityAuthenticationService _identityAuthenticationService) : IClientAccountAdminDeletionService
{
    /// <summary>
    /// Removes a client portal account after verifying that the user exists and is a client.
    /// </summary>
    /// <param name="clientUserId">Identifier of the client account to delete.</param>
    /// <param name="cancellationToken">Propagation token to cancel the command.</param>
    /// <returns>Success when the account is torn down; an error when validation or deletion fails.</returns>
    public async Task<ErrorOr<Success>> DeleteByAdminAsync(
        Guid clientUserId,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<string> roles = await _userRoleReadService.GetRoleNamesAsync(
            clientUserId,
            cancellationToken);

        if (roles.Count == 0)
        {
            return Error.NotFound(
                code: "Auth.ClientUserNotFound",
                description: "Client user was not found.");
        }

        if (!AuthPortalClassifier.IsClientPortalUser(roles))
        {
            return Error.Forbidden(
                code: "Auth.NotClientPortalUser",
                description: "The target account is not a client-portal user.");
        }

        return await _identityAuthenticationService.TeardownClientAccountAsync(
            clientUserId,
            cancellationToken);
    }
}
