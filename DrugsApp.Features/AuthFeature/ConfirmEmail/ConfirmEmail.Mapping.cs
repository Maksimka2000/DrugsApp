using System;

namespace DrugsApp.Features.AuthFeature.ConfirmEmail;

/// <summary>
/// Mapping helpers for confirm email requests.
/// </summary>
public static class ConfirmEmailMapping
{
    /// <summary>
    /// Maps the incoming request to a command envelope.
    /// </summary>
    /// <param name="request">Confirm email request payload.</param>
    /// <param name="userId">Identifier of the user confirming email.</param>
    /// <returns>Command for MediatR handling.</returns>
    public static ConfirmEmailCommand MapToCommand(
        this ConfirmEmailRequest request,
        Guid userId) => new(userId, request);
}
