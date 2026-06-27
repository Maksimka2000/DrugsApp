using System;

namespace DrugsApp.Features.AuthFeature.ResetPassword;

/// <summary>
/// Mapping helpers for password reset.
/// </summary>
public static class ResetPasswordMapping
{
    /// <summary>
    /// Maps the incoming reset payload to a command envelope.
    /// </summary>
    /// <param name="request">Reset password request payload.</param>
    /// <param name="userId">Identifier of the user resetting password.</param>
    /// <returns>Command for MediatR handling.</returns>
    public static ResetPasswordCommand MapToCommand(
        this ResetPasswordRequest request,
        Guid userId) =>
        new(userId, request);
}
