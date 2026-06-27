using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Features.AccountFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AccountFeature.ConfirmLoginEmailChange;

/// <summary>
/// Request payload used to confirm a pending login email change.
/// </summary>
public sealed record ConfirmLoginEmailChangeRequest
{
    /// <summary>
    /// Identifier of the user confirming the change.
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// Confirmation token issued during the email change request.
    /// </summary>
    public string Token { get; init; }

    /// <summary>
    /// Creates a confirmation request with required identifiers.
    /// </summary>
    /// <param name="userId">Identifier of the user confirming the change.</param>
    /// <param name="token">Confirmation token issued to the user.</param>
    public ConfirmLoginEmailChangeRequest(Guid userId, string token)
    {
        UserId = userId;
        Token = token;
    }
}

/// <summary>
/// Command envelope for confirming a pending login email change.
/// </summary>
public sealed record ConfirmLoginEmailChangeCommand(
    Guid UserId,
    string Token) : IRequest<ErrorOr<ConfirmLoginEmailChangeResponse>>;

internal sealed class ConfirmLoginEmailChangeHandler(IIdentityAuthenticationService _identityAuthenticationService)
    : IRequestHandler<ConfirmLoginEmailChangeCommand, ErrorOr<ConfirmLoginEmailChangeResponse>>
{
    public async Task<ErrorOr<ConfirmLoginEmailChangeResponse>> Handle(ConfirmLoginEmailChangeCommand command, CancellationToken cancellationToken)
    {
        ErrorOr<ConfirmLoginEmailChangeResult> result = await _identityAuthenticationService.ConfirmEmailChangeAsync(
            command.UserId,
            command.Token,
            cancellationToken);

        if (result.IsError)
        {
            return result.Errors;
        }

        return new ConfirmLoginEmailChangeResponse(
            Email: result.Value.Email,
            EmailChangeConfirmed: result.Value.EmailChangeConfirmed);
    }
}
