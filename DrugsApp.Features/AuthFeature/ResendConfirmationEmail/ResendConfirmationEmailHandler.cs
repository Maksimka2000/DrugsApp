using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature.Interfaces;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AuthFeature.ResendConfirmationEmail;

internal sealed class ResendConfirmationEmailHandler(
    IIdentityAuthenticationService _identityAuthenticationService)
    : IRequestHandler<ResendConfirmationEmailCommand, ErrorOr<ResendConfirmationEmailResponse?>>
{
    public async Task<ErrorOr<ResendConfirmationEmailResponse?>> Handle(
        ResendConfirmationEmailCommand command,
        CancellationToken cancellationToken)
    {
        ErrorOr<AuthRegistrationResult?> result = await _identityAuthenticationService.ResendConfirmationEmailAsync(
            command.Request.Email,
            cancellationToken);

        if (result.IsError){
            return result.Errors;
        }

        return result.Value.MapToResponse();
    }
}
