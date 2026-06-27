using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Enums;
using DrugsApp.Domain.UserProfileFeature.Repositories;
using ErrorOr;

namespace DrugsApp.Features.UserProfileFeature.ApplicationServices;

/// <summary>
/// Application service for creating or updating reproductive health profiles.
/// </summary>
internal sealed class ReproductiveHealthProfileApplicationService(
    IUserProfileRepository _userProfileRepository,
    IClientReproductiveHealthProfileRepository _clientReproductiveHealthProfileRepository)
    : IReproductiveHealthProfileApplicationService
{
    /// <summary>
    /// Upserts reproductive health profile for a user with gender validation.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="request">Upsert payload.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Upsert result or errors.</returns>
    public async Task<ErrorOr<ReproductiveHealthProfileUpsertResult>> UpsertAsync(
        Guid userId,
        ReproductiveHealthProfileUpsertRequest request,
        CancellationToken cancellationToken)
    {
        UserProfile? userProfile = await _userProfileRepository.GetByUserIdAsync(userId, cancellationToken);
        if (userProfile is null)
        {
            return Error.NotFound(
                description: "User profile not found.",
                metadata: new()
                {
                    ["resourceType"] = "UserProfile",
                    ["resourceId"] = userId.ToString()
                });
        }

        if (userProfile.Gender != ProfileGender.female)
        {
            return Error.Forbidden(
                description: "Reproductive health profile is allowed only for female gender.",
                metadata: new()
                {
                    ["resourceType"] = "UserProfile",
                    ["resourceId"] = userId.ToString(),
                    ["requiresGender"] = ProfileGender.female.ToString()
                });
        }

        ClientReproductiveHealthProfile? existing = await _clientReproductiveHealthProfileRepository.GetTrackedByUserIdAsync(
            userId,
            cancellationToken);

        if (existing is null)
        {
            ErrorOr<ClientReproductiveHealthProfile> created = ClientReproductiveHealthProfile.Create(
                userId,
                request.CurrentlyPregnant,
                request.PlanningToGetPregnant,
                request.PregnancyPlanningUnknown,
                request.Menopause,
                request.ContraceptionMethodCode);

            if (created.IsError)
            {
                return created.Errors;
            }

            await _clientReproductiveHealthProfileRepository.AddAsync(created.Value, cancellationToken);

            return new ReproductiveHealthProfileUpsertResult(created.Value, true);
        }

        ErrorOr<Success> updateResult = existing.Update(
            request.CurrentlyPregnant,
            request.PlanningToGetPregnant,
            request.PregnancyPlanningUnknown,
            request.Menopause,
            request.ContraceptionMethodCode);

        if (updateResult.IsError)
        {
            return updateResult.Errors;
        }

        return new ReproductiveHealthProfileUpsertResult(existing, false);
    }
}
