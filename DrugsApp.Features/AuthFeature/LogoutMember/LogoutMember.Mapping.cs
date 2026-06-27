namespace DrugsApp.Features.AuthFeature.LogoutMember;

/// <summary>
/// Mapping helpers for member logout.
/// </summary>
public static class LogoutMemberMapping
{
    /// <summary>
    /// Maps the incoming request to a command envelope.
    /// </summary>
    /// <param name="request">Logout request payload.</param>
    /// <returns>Command for MediatR handling.</returns>
    public static LogoutMemberCommand MapToCommand(this LogoutMemberRequest request) => new(request);
}
