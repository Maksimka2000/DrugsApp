using DrugsApp.Domain.AuthFeature.Interfaces;

namespace DrugsApp.Features.AuthFeature.RegisterMember;

public static class RegisterMemberMapping
{
    public static RegisterMemberCommand MapToCommand(this RegisterMemberRequest request) =>
        new(request);

    public static RegisterMemberResponse MapToResponse(this AuthRegistrationResult result) =>
        new(
            UserId: result.UserId,
            Email: result.Email);
}
