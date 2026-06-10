namespace DrugsApp.Features.AuthFeature.LogoutMember;

public static class LogoutMemberMapping
{
    public static LogoutMemberCommand MapToCommand(this LogoutMemberRequest request) => new(request);
}
