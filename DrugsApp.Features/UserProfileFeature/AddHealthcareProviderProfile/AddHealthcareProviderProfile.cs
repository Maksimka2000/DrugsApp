using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Enums;
using DrugsApp.Domain.UserProfileFeature.Repositories;
using DrugsApp.Features.UserProfileFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.UserProfileFeature.AddHealthcareProviderProfile;

public sealed record AddHealthcareProviderProfileRequest(
    HealthcareProviderProfession? ProfessionCode,
    string? OtherProfessionLabel,
    string? WorkplaceLabel,
    string? ShortDescription);

public sealed record AddHealthcareProviderProfileCommand(
    Guid UserId,
    AddHealthcareProviderProfileRequest Request) : IRequest<ErrorOr<HealthcareProviderProfileResponse>>;

internal sealed class AddHealthcareProviderProfileHandler(
    IUserProfileRepository _userProfileRepository,
    IHealthcareProviderProfileRepository _healthcareProviderProfileRepository,
    IIdentityAuthenticationService _identityAuthenticationService)
    : IRequestHandler<AddHealthcareProviderProfileCommand, ErrorOr<HealthcareProviderProfileResponse>>
{
    public async Task<ErrorOr<HealthcareProviderProfileResponse>> Handle(AddHealthcareProviderProfileCommand command, CancellationToken cancellationToken)
    {
        UserProfile? profile = await _userProfileRepository.GetByUserIdAsync(command.UserId, cancellationToken);
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

        bool exists = await _healthcareProviderProfileRepository.GetTrackedByUserIdAsync(command.UserId, cancellationToken) is not null;
        if (exists)
        {
            return Error.Conflict(
                description: "Healthcare provider profile already exists.",
                metadata: new()
                {
                    ["resourceType"] = "HealthcareProviderProfile",
                    ["resourceId"] = command.UserId.ToString()
                });
        }

        ErrorOr<HealthcareProviderProfile> createResult = HealthcareProviderProfile.Create(
            userId: command.UserId,
            professionCode: command.Request.ProfessionCode,
            otherProfessionLabel: command.Request.OtherProfessionLabel,
            workplaceLabel: command.Request.WorkplaceLabel,
            shortDescription: command.Request.ShortDescription);

        if (createResult.IsError)
        {
            return createResult.Errors;
        }

        await _healthcareProviderProfileRepository.AddAsync(createResult.Value, cancellationToken);

        ErrorOr<Success> roleResult = await _identityAuthenticationService.AssignHealthcareProviderRoleAsync(
            command.UserId,
            cancellationToken);

        if (roleResult.IsError)
        {
            return roleResult.Errors;
        }

        return HealthcareProviderProfileResponse.From(createResult.Value);
    }
}
