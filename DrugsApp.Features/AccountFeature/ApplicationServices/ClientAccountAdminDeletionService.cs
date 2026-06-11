using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AccountFeature.Interfaces;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Domain.AuthFeature.Interfaces;
using ErrorOr;

namespace DrugsApp.Features.AccountFeature.ApplicationServices;

public sealed class ClientAccountAdminDeletionService(
    IUserRoleReadService _userRoleReadService,
    IIdentityAuthenticationService _identityAuthenticationService) : IClientAccountAdminDeletionService
{
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
