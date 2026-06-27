using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature.Interfaces;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AccountFeature.ChangeAccountPassword;

/// <summary>
/// Request payload for changing the current user's password.
/// </summary>
public sealed record ChangeAccountPasswordRequest
{
    /// <summary>
    /// Existing password used to authorize the change.
    /// </summary>
    public string CurrentPassword { get; init; }

    /// <summary>
    /// Desired new password.
    /// </summary>
    public string NewPassword { get; init; }

    /// <summary>
    /// Confirmation of the desired new password.
    /// </summary>
    public string ConfirmNewPassword { get; init; }

    /// <summary>
    /// Creates a change password request with required fields.
    /// </summary>
    /// <param name="currentPassword">Existing password used for verification.</param>
    /// <param name="newPassword">Desired new password.</param>
    /// <param name="confirmNewPassword">Confirmation of the desired new password.</param>
    public ChangeAccountPasswordRequest(
        string currentPassword,
        string newPassword,
        string confirmNewPassword)
    {
        CurrentPassword = currentPassword;
        NewPassword = newPassword;
        ConfirmNewPassword = confirmNewPassword;
    }
}

/// <summary>
/// Command envelope for changing a user's password.
/// </summary>
public sealed record ChangeAccountPasswordCommand(
    Guid UserId,
    ChangeAccountPasswordRequest Request) : IRequest<ErrorOr<Success>>;

internal sealed class ChangeAccountPasswordHandler(IIdentityAuthenticationService _identityAuthenticationService)
    : IRequestHandler<ChangeAccountPasswordCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(ChangeAccountPasswordCommand command, CancellationToken cancellationToken)
    {
        return await _identityAuthenticationService.ChangePasswordAsync(
            command.UserId,
            command.Request.CurrentPassword,
            command.Request.NewPassword,
            cancellationToken);
    }
}
