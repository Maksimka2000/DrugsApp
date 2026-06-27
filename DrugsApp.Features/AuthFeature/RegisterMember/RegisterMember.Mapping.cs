using DrugsApp.Domain.AuthFeature.Interfaces;

namespace DrugsApp.Features.AuthFeature.RegisterMember;

/// <summary>
/// Mapping helpers for member registration.
/// </summary>
public static class RegisterMemberMapping
{
    /// <summary>
    /// Maps the incoming request to a command envelope.
    /// </summary>
    /// <param name="request">Registration request payload.</param>
    /// <returns>Command for MediatR handling.</returns>
    public static RegisterMemberCommand MapToCommand(this RegisterMemberRequest request) =>
        new(request);

    /// <summary>
    /// Maps registration result to the API response.
    /// </summary>
    /// <param name="result">Registration outcome with identifiers.</param>
    /// <returns>Response wrapper.</returns>
    public static RegisterMemberResponse MapToResponse(this AuthRegistrationResult result) =>
        new(
            UserId: result.UserId,
            Email: result.Email);
}
