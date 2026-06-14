using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Enums;
using DrugsApp.Features.UserProfileFeature.Shared;
using MediatR;
using DrugsApp.Domain.UserProfileFeature.Repositories;
using ErrorOr;

namespace DrugsApp.Features.UserProfileFeature.AddReproductiveHealthProfile;

public sealed record AddReproductiveHealthProfileRequest(
    bool CurrentlyPregnant,
    bool PlanningToGetPregnant,
    bool PregnancyPlanningUnknown,
    bool Menopause,
    ContraceptionMethodCode? ContraceptionMethodCode);

public sealed record AddReproductiveHealthProfileCommand(
    Guid UserId,
    AddReproductiveHealthProfileRequest Request) : IRequest<ErrorOr<ReproductiveHealthProfileResponse>>;

internal sealed class AddReproductiveHealthProfileHandler(
    IUserProfileRepository _userProfileRepository,
    IClientReproductiveHealthProfileRepository _clientReproductiveHealthProfileRepository)
    : IRequestHandler<AddReproductiveHealthProfileCommand, ErrorOr<ReproductiveHealthProfileResponse>>
{
    public async Task<ErrorOr<ReproductiveHealthProfileResponse>> Handle(AddReproductiveHealthProfileCommand command, CancellationToken cancellationToken)
    {
        UserProfile? userProfile = await _userProfileRepository.GetByUserIdAsync(command.UserId, cancellationToken);
        if (userProfile is null)
        {
            return Error.NotFound(
                description: "User profile not found.",
                metadata: new()
                {
                    ["resourceType"] = "UserProfile",
                    ["resourceId"] = command.UserId.ToString()
                });
        }

        if (userProfile.Gender != ProfileGender.female)
        {
            return Error.Forbidden(
                description: "Reproductive health profile can only be created for female gender.",
                metadata: new()
                {
                    ["resourceType"] = "UserProfile",
                    ["resourceId"] = command.UserId.ToString(),
                    ["requiresGender"] = ProfileGender.female.ToString()
                });
        }

        bool exists = await _clientReproductiveHealthProfileRepository.ExistsByUserIdAsync(command.UserId, cancellationToken);
        if (exists)
        {
            return Error.Conflict(
                description: "Reproductive health profile already exists.",
                metadata: new()
                {
                    ["resourceType"] = "ClientReproductiveHealthProfile",
                    ["resourceId"] = command.UserId.ToString()
                });
        }

        ErrorOr<ClientReproductiveHealthProfile> createResult = ClientReproductiveHealthProfile.Create(
            command.UserId,
            command.Request.CurrentlyPregnant,
            command.Request.PlanningToGetPregnant,
            command.Request.PregnancyPlanningUnknown,
            command.Request.Menopause,
            command.Request.ContraceptionMethodCode);

        if (createResult.IsError)
        {
            return createResult.Errors;
        }

        await _clientReproductiveHealthProfileRepository.AddAsync(createResult.Value, cancellationToken);

        return ReproductiveHealthProfileResponse.From(createResult.Value);
    }
}
