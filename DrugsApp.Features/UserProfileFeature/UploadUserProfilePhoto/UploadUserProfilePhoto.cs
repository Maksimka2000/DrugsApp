using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.Common;
using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Repositories;
using DrugsApp.Features.UserProfileFeature.Shared;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace DrugsApp.Features.UserProfileFeature.UploadUserProfilePhoto;

public sealed record UploadUserProfilePhotoRequest(IFormFile File);

public sealed record UploadUserProfilePhotoCommand(Guid UserId, IFormFile File) : IRequest<ErrorOr<UserProfilePhotoResponse>>;

internal sealed class UploadUserProfilePhotoHandler(
    IUserProfileRepository _userProfileRepository,
    IBlobService _blobService) : IRequestHandler<UploadUserProfilePhotoCommand, ErrorOr<UserProfilePhotoResponse>>
{
    public async Task<ErrorOr<UserProfilePhotoResponse>> Handle(UploadUserProfilePhotoCommand command, CancellationToken cancellationToken)
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

        await using Stream stream = command.File.OpenReadStream();
        Guid storageKey = await _blobService.UploadAsync(
            stream,
            command.File.ContentType,
            cancellationToken);

        ErrorOr<Success> setResult = profile.SetProfilePhotoStorageKey(storageKey.ToString());
        if (setResult.IsError)
        {
            return setResult.Errors;
        }

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
                // Best-effort cleanup per plan.
            }
        }

        return new UserProfilePhotoResponse("/api/user-profile/photo");
    }
}
