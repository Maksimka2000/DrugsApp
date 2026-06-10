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

namespace DrugsApp.Features.UserProfileFeature.GetUserProfile;

public sealed record GetUserProfileQuery(Guid UserId, string Email) : IRequest<ErrorOr<UserProfileResponse>>;

internal sealed class GetUserProfileHandler(
    IUserProfileRepository _userProfileRepository,
    IClientReproductiveHealthProfileRepository _clientReproductiveHealthProfileRepository,
    IPendingEmailChangeRepository _pendingEmailChangeRepository)
    : IRequestHandler<GetUserProfileQuery, ErrorOr<UserProfileResponse>>
{
    public async Task<ErrorOr<UserProfileResponse>> Handle(GetUserProfileQuery query, CancellationToken cancellationToken)
    {
        UserProfile? profile = await _userProfileRepository.GetByUserIdAsync(query.UserId, cancellationToken);
        if (profile is null)
        {
            return Error.NotFound(
                description: "User profile not found.",
                metadata: new()
                {
                    ["resourceType"] = "UserProfile",
                    ["resourceId"] = query.UserId.ToString()
                });
        }

        bool hasReproductiveHealthProfile = await _clientReproductiveHealthProfileRepository.ExistsByUserIdAsync(
            query.UserId,
            cancellationToken);

        PendingEmailChange? pending = await _pendingEmailChangeRepository.GetActiveByUserIdAsync(query.UserId, cancellationToken);

        string? profilePhotoUrl = profile.ProfilePhotoStorageKey is null
            ? null
            : "/api/user-profile/photo";

        var response = UserProfileResponse.From(
            profile,
            hasReproductiveHealthProfile,
            pending?.NewEmail,
            profilePhotoUrl);

        return response;
    }
}
