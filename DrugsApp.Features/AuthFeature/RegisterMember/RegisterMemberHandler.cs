using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Domain.UserProfileFeature.Enums;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AuthFeature.RegisterMember;

internal sealed class RegisterMemberHandler(
    IIdentityAuthenticationService _identityAuthenticationService)
    : IRequestHandler<RegisterMemberCommand, ErrorOr<RegisterMemberResponse>>
{
    public async Task<ErrorOr<RegisterMemberResponse>> Handle(
        RegisterMemberCommand command,
        CancellationToken cancellationToken)
    {
        ErrorOr<AuthRegistrationResult> result = await _identityAuthenticationService.RegisterAsync(
            command.Request.Email,
            command.Request.Password,
            new UserProfilePersonalData(
                FirstName: command.Request.FirstName,
                LastName: command.Request.LastName,
                DateOfBirth: command.Request.DateOfBirth,
                Gender: Enum.Parse<ProfileGender>(command.Request.Gender, ignoreCase: true)),
            cancellationToken);

        if (result.IsError)
        {
            return result.Errors;
        }

        return result.Value.MapToResponse();
    }
}
