namespace DrugsApp.Features.AuthFeature.ForgotPassword;

/// <summary>
/// Mapping helpers for forgot-password requests.
/// </summary>
public static class ForgotPasswordMapping
{
    /// <summary>
    /// Maps the incoming request to a command envelope.
    /// </summary>
    /// <param name="request">Forgot password request payload.</param>
    /// <returns>Command for MediatR handling.</returns>
    public static ForgotPasswordCommand MapToCommand(this ForgotPasswordRequest request) =>
        new(request);
}
