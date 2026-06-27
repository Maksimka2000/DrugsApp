using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.UserProfileFeature.Enums;
using DrugsApp.Features.UserProfileFeature.ApplicationServices;
using DrugsApp.Features.UserProfileFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.UserProfileFeature.UpdateHealthcareProviderProfile;

/// <summary>
/// Request payload to update provider profile details.
/// </summary>
/// <param name="ProfessionCode">Provider profession code.</param>
/// <param name="OtherProfessionLabel">Custom profession label.</param>
/// <param name="WorkplaceLabel">Workplace label.</param>
/// <param name="ShortDescription">Short description.</param>
/// <param name="OnboardingCompletedAtUtc">Onboarding completion timestamp.</param>
public sealed record UpdateHealthcareProviderProfileRequest(
    HealthcareProviderProfession? ProfessionCode,
    string? OtherProfessionLabel,
    string? WorkplaceLabel,
    string? ShortDescription,
    DateTime? OnboardingCompletedAtUtc);

/// <summary>
/// Command to upsert a healthcare provider profile.
/// </summary>
/// <param name="UserId">User identifier.</param>
/// <param name="Request">Update payload.</param>
public sealed record UpdateHealthcareProviderProfileCommand(
    Guid UserId,
    UpdateHealthcareProviderProfileRequest Request) : IRequest<ErrorOr<HealthcareProviderProfileResponse>>;

internal sealed class UpdateHealthcareProviderProfileHandler(
    IHealthcareProviderProfileApplicationService _service)
    : IRequestHandler<UpdateHealthcareProviderProfileCommand, ErrorOr<HealthcareProviderProfileResponse>>
{
    /// <summary>
    /// Upserts healthcare provider profile data for the user.
    /// </summary>
    /// <param name="command">Command containing user id and payload.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Upserted profile response or errors.</returns>
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
