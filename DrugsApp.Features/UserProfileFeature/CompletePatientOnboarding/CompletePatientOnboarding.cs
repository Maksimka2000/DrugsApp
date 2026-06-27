using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature.Entities;
using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Repositories;
using DrugsApp.Features.UserProfileFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.UserProfileFeature.CompletePatientOnboarding;

/// <summary>
/// Command to mark patient onboarding as completed.
/// </summary>
/// <param name="UserId">User identifier.</param>
/// <param name="Email">User email.</param>
public sealed record CompletePatientOnboardingCommand(Guid UserId, string Email)
    : IRequest<ErrorOr<UserProfileResponse>>;

internal sealed class CompletePatientOnboardingHandler(
    IUserProfileRepository userProfileRepository,
    IClientReproductiveHealthProfileRepository reproductiveHealthProfileRepository,
    IPendingEmailChangeRepository pendingEmailChangeRepository)
    : IRequestHandler<CompletePatientOnboardingCommand, ErrorOr<UserProfileResponse>>
{
    /// <summary>
    /// Completes onboarding, updates profile state, and returns profile response.
    /// </summary>
    /// <param name="command">Onboarding completion command.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Updated profile response or errors.</returns>
    public async Task<ErrorOr<UserProfileResponse>> Handle(
        CompletePatientOnboardingCommand command,
        CancellationToken cancellationToken)
    {
        UserProfile? profile = await userProfileRepository.GetTrackedByUserIdAsync(command.UserId, cancellationToken);
        if (profile is null)
        {
            return Error.NotFound(
                description: "User profile not found.",
                metadata: new()
                {
                    ["resourceType"] = "UserProfile",
                    ["resourceId"] = command.UserId.ToString(),
                });
        }

        profile.MarkOnboardingCompleted(DateTime.UtcNow);

        bool hasReproductiveHealthProfile = await reproductiveHealthProfileRepository.ExistsByUserIdAsync(
            command.UserId,
            cancellationToken);

        PendingEmailChange? pending = await pendingEmailChangeRepository.GetActiveByUserIdAsync(
            command.UserId,
            cancellationToken);

        string? profilePhotoUrl = profile.ProfilePhotoStorageKey is null
            ? null
            : "/api/user-profile/photo";

        return UserProfileResponse.From(
            profile,
            hasReproductiveHealthProfile,
            pending?.NewEmail,
            profilePhotoUrl);
    }
}
