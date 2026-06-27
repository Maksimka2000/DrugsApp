using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Features.AccountFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AccountFeature.RequestLoginEmailChange;

/// <summary>
/// Request payload for initiating a login email change.
/// </summary>
public sealed record RequestLoginEmailChangeRequest
{
    /// <summary>
    /// New login email to assign to the user.
    /// </summary>
    public string NewEmail { get; init; }

    /// <summary>
    /// Current password used to authorize the change.
    /// </summary>
    public string CurrentPassword { get; init; }

    /// <summary>
    /// Creates an email change request with required credentials.
    /// </summary>
    /// <param name="newEmail">Desired new login email.</param>
    /// <param name="currentPassword">Current password for verification.</param>
    public RequestLoginEmailChangeRequest(string newEmail, string currentPassword)
    {
        NewEmail = newEmail;
        CurrentPassword = currentPassword;
    }
}

/// <summary>
/// Command envelope for requesting a login email change.
/// </summary>
public sealed record RequestLoginEmailChangeCommand(
    Guid UserId,
    RequestLoginEmailChangeRequest Request) : IRequest<ErrorOr<RequestLoginEmailChangeResponse>>;

internal sealed class RequestLoginEmailChangeHandler(IIdentityAuthenticationService _identityAuthenticationService)
    : IRequestHandler<RequestLoginEmailChangeCommand, ErrorOr<RequestLoginEmailChangeResponse>>
{
    public async Task<ErrorOr<RequestLoginEmailChangeResponse>> Handle(RequestLoginEmailChangeCommand command, CancellationToken cancellationToken)
    {
        ErrorOr<RequestLoginEmailChangeResult> result = await _identityAuthenticationService.RequestEmailChangeAsync(
            command.UserId,
            command.Request.NewEmail,
            command.Request.CurrentPassword,
            cancellationToken);

        if (result.IsError)
        {
            return result.Errors;
        }

        return new RequestLoginEmailChangeResponse(
            Email: result.Value.Email,
            PendingEmail: result.Value.PendingEmail,
            VerificationRequired: result.Value.VerificationRequired);
    }
}
