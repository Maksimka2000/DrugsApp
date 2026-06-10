using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.Common;
using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.UserProfileFeature.DownloadUserProfilePhoto;

public sealed record DownloadUserProfilePhotoQuery(Guid UserId) : IRequest<ErrorOr<FileResponse>>;

internal sealed class DownloadUserProfilePhotoHandler(
    IUserProfileRepository _userProfileRepository,
    IBlobService _blobService) : IRequestHandler<DownloadUserProfilePhotoQuery, ErrorOr<FileResponse>>
{
    public async Task<ErrorOr<FileResponse>> Handle(DownloadUserProfilePhotoQuery query, CancellationToken cancellationToken)
    {
        UserProfile? profile = await _userProfileRepository.GetByUserIdAsync(query.UserId, cancellationToken);
        if (profile is null || profile.ProfilePhotoStorageKey is null)
        {
            return Error.NotFound(
                description: "Profile photo not found.",
                metadata: new()
                {
                    ["resourceType"] = "UserProfilePhoto",
                    ["resourceId"] = query.UserId.ToString()
                });
        }

        try
        {
            if (!Guid.TryParse(profile.ProfilePhotoStorageKey, out Guid storageKey))
            {
                return Error.NotFound(
                    description: "Profile photo not found.",
                    metadata: new()
                    {
                        ["resourceType"] = "UserProfilePhoto",
                        ["resourceId"] = query.UserId.ToString()
                    });
            }

            FileResponse file = await _blobService.DownloadAsync(storageKey, cancellationToken);
            return file;
        }
        catch (FileNotFoundException)
        {
            return Error.NotFound(
                description: "Profile photo not found.",
                metadata: new()
                {
                    ["resourceType"] = "UserProfilePhoto",
                    ["resourceId"] = query.UserId.ToString()
                });
        }
    }
}
