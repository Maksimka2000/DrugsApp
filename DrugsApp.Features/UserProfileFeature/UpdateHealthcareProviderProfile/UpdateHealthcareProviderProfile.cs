using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.UserProfileFeature.Enums;
using DrugsApp.Features.UserProfileFeature.ApplicationServices;
using DrugsApp.Features.UserProfileFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.UserProfileFeature.UpdateHealthcareProviderProfile;

public sealed record UpdateHealthcareProviderProfileRequest(
    HealthcareProviderProfession? ProfessionCode,
    string? OtherProfessionLabel,
    string? WorkplaceLabel,
    string? ShortDescription,
    DateTime? OnboardingCompletedAtUtc);

public sealed record UpdateHealthcareProviderProfileCommand(
    Guid UserId,
    UpdateHealthcareProviderProfileRequest Request) : IRequest<ErrorOr<HealthcareProviderProfileResponse>>;

internal sealed class UpdateHealthcareProviderProfileHandler(
    IHealthcareProviderProfileApplicationService _service)
    : IRequestHandler<UpdateHealthcareProviderProfileCommand, ErrorOr<HealthcareProviderProfileResponse>>
{
    public async Task<ErrorOr<HealthcareProviderProfileResponse>> Handle(UpdateHealthcareProviderProfileCommand command, CancellationToken cancellationToken)
    {
        ErrorOr<HealthcareProviderProfileUpsertResult> result = await _service.UpsertFromOnboardingAsync(
            command.UserId,
            new HealthcareProviderProfileUpsertRequest(
                command.Request.ProfessionCode,
                command.Request.OtherProfessionLabel,
                command.Request.WorkplaceLabel,
                command.Request.ShortDescription,
                command.Request.OnboardingCompletedAtUtc),
            cancellationToken);

        if (result.IsError)
        {
            return result.Errors;
        }

        return HealthcareProviderProfileResponse.From(result.Value.Profile);
    }
}
