using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.UserProfileFeature.Enums;
using DrugsApp.Features.UserProfileFeature.Shared;
using DrugsApp.Features.UserProfileFeature.ApplicationServices;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.UserProfileFeature.UpdateReproductiveHealthProfile;

/// <summary>
/// Request payload to update reproductive health profile.
/// </summary>
/// <param name="CurrentlyPregnant">Pregnancy flag.</param>
/// <param name="PlanningToGetPregnant">Planning to get pregnant flag.</param>
/// <param name="PregnancyPlanningUnknown">Unknown planning flag.</param>
/// <param name="Menopause">Menopause flag.</param>
/// <param name="ContraceptionMethodCode">Contraception method code.</param>
public sealed record UpdateReproductiveHealthProfileRequest(
    bool CurrentlyPregnant,
    bool PlanningToGetPregnant,
    bool PregnancyPlanningUnknown,
    bool Menopause,
    ContraceptionMethodCode? ContraceptionMethodCode);

/// <summary>
/// Command to upsert reproductive health profile for a user.
/// </summary>
/// <param name="UserId">User identifier.</param>
/// <param name="Request">Update payload.</param>
public sealed record UpdateReproductiveHealthProfileCommand(
    Guid UserId,
    UpdateReproductiveHealthProfileRequest Request) : IRequest<ErrorOr<ReproductiveHealthProfileResponse>>;

internal sealed class UpdateReproductiveHealthProfileHandler(
    IReproductiveHealthProfileApplicationService _service)
    : IRequestHandler<UpdateReproductiveHealthProfileCommand, ErrorOr<ReproductiveHealthProfileResponse>>
{
    /// <summary>
    /// Upserts reproductive health profile data for the user.
    /// </summary>
    /// <param name="command">Command containing user id and payload.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Updated profile response or errors.</returns>
    public async Task<ErrorOr<ReproductiveHealthProfileResponse>> Handle(UpdateReproductiveHealthProfileCommand command, CancellationToken cancellationToken)
    {
        ErrorOr<ReproductiveHealthProfileUpsertResult> result = await _service.UpsertAsync(
            command.UserId,
            new ReproductiveHealthProfileUpsertRequest(
                command.Request.CurrentlyPregnant,
                command.Request.PlanningToGetPregnant,
                command.Request.PregnancyPlanningUnknown,
                command.Request.Menopause,
                command.Request.ContraceptionMethodCode),
            cancellationToken);

        if (result.IsError)
        {
            return result.Errors;
        }

        return ReproductiveHealthProfileResponse.From(result.Value.Profile);
    }
}
