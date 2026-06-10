using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature.Entities;
using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Enums;
using DrugsApp.Domain.UserProfileFeature.Repositories;
using DrugsApp.Features.UserProfileFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.UserProfileFeature.UpdateUserProfile;

public sealed record UpdateUserProfileRequest(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Gender,
    bool? ConfirmReproductiveHealthDeletion);

public sealed record UpdateUserProfileCommand(
    Guid UserId,
    string Email,
    UpdateUserProfileRequest Request) : IRequest<ErrorOr<UserProfileResponse>>;

internal sealed class UpdateUserProfileHandler(
    IUserProfileRepository _userProfileRepository,
    IClientReproductiveHealthProfileRepository _clientReproductiveHealthProfileRepository,
    IPendingEmailChangeRepository _pendingEmailChangeRepository)
    : IRequestHandler<UpdateUserProfileCommand, ErrorOr<UserProfileResponse>>
{
    public async Task<ErrorOr<UserProfileResponse>> Handle(UpdateUserProfileCommand command, CancellationToken cancellationToken)
    {
        UserProfile? profile = await _userProfileRepository.GetTrackedByUserIdAsync(command.UserId, cancellationToken);
        if (profile is null)
        {
            return Error.NotFound(
                description: "User profile not found.",
                metadata: new()
                {
                    ["resourceType"] = "UserProfile",
                    ["resourceId"] = command.UserId.ToString()
                });
        }

        ProfileGender newGender = Enum.Parse<ProfileGender>(command.Request.Gender, ignoreCase: true);
        bool genderChangedAwayFromFemale = profile.Gender == ProfileGender.female && newGender != ProfileGender.female;

        bool hasReproductive = await _clientReproductiveHealthProfileRepository.ExistsByUserIdAsync(
            command.UserId,
            cancellationToken);

        if (genderChangedAwayFromFemale && hasReproductive && command.Request.ConfirmReproductiveHealthDeletion is not true)
        {
            return Error.Conflict(
                description: "Reproductive health profile deletion requires confirmation.",
                metadata: new()
                {
                    ["requiresConfirmation"] = true
                });
        }

        ErrorOr<Success> updateResult = profile.UpdatePersonalData(
            command.Request.FirstName,
            command.Request.LastName,
            command.Request.DateOfBirth,
            newGender);

        if (updateResult.IsError)
        {
            return updateResult.Errors;
        }

        if (genderChangedAwayFromFemale && hasReproductive && command.Request.ConfirmReproductiveHealthDeletion is true)
        {
            await _clientReproductiveHealthProfileRepository.RemoveByUserIdAsync(command.UserId, cancellationToken);
            hasReproductive = false;
        }

        PendingEmailChange? pending = await _pendingEmailChangeRepository.GetActiveByUserIdAsync(command.UserId, cancellationToken);

        string? profilePhotoUrl = profile.ProfilePhotoStorageKey is null
            ? null
            : "/api/user-profile/photo";

        bool showMedicalReviewHint = newGender == ProfileGender.female && profile.Gender != ProfileGender.female;

        var response = UserProfileResponse.From(
            profile,
            hasReproductive,
            pending?.NewEmail,
            profilePhotoUrl,
            showMedicalReviewHint);

        return response;
    }
}
