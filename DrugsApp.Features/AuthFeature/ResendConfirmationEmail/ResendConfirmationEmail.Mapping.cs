using DrugsApp.Domain.AuthFeature.Interfaces;

namespace DrugsApp.Features.AuthFeature.ResendConfirmationEmail;

/// <summary>
/// Mapping helpers for resend confirmation email use case.
/// </summary>
public static class ResendConfirmationEmailMapping
{
    /// <summary>
    /// Maps a <see cref="ResendConfirmationEmailRequest"/> to a <see cref="ResendConfirmationEmailCommand"/>.
    /// </summary>
    /// <param name="request">The resend confirmation email request containing the user's email.</param>
    /// <returns>A command object containing the request data.</returns>
    public static ResendConfirmationEmailCommand MapToCommand(
        this ResendConfirmationEmailRequest request) =>
        new(request);

    /// <summary>
    /// Maps an <see cref="AuthRegistrationResult"/> to a <see cref="ResendConfirmationEmailResponse"/>.
    /// </summary>
    /// <param name="result">The authentication registration result containing user details.</param>
    /// <returns>A response object containing the user ID and email.</returns>
    public static ResendConfirmationEmailResponse MapToResponse(this AuthRegistrationResult result) =>
        new(
            UserId: result.UserId,
            Email: result.Email);
}
