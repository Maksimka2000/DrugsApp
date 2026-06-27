using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.Common;
using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.UserProfileFeature.RemoveUserProfilePhoto;

/// <summary>
/// Command to remove a user's profile photo.
/// </summary>
/// <param name="UserId">User identifier.</param>
public sealed record RemoveUserProfilePhotoCommand(Guid UserId) : IRequest<ErrorOr<Success>>;

internal sealed class RemoveUserProfilePhotoHandler(
    IUserProfileRepository _userProfileRepository,
    IBlobService _blobService) : IRequestHandler<RemoveUserProfilePhotoCommand, ErrorOr<Success>>
{
    /// <summary>
    /// Removes profile photo metadata and deletes stored file if present.
    /// </summary>
    /// <param name="command">Remove photo command.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Success or errors.</returns>
    public async Task<ErrorOr<Success>> Handle(RemoveUserProfilePhotoCommand command, CancellationToken cancellationToken)
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

        string? previousKey = profile.ProfilePhotoStorageKey;
        profile.RemoveProfilePhoto();

        if (!string.IsNullOrWhiteSpace(previousKey))
        {
            try
            {
                if (Guid.TryParse(previousKey, out Guid previousKeyGuid))
                {
                    await _blobService.DeleteAsync(previousKeyGuid, cancellationToken);
                }
            }
            catch
            {
                // Best-effort cleanup.
            }
        }

        return Result.Success;
    }
}
