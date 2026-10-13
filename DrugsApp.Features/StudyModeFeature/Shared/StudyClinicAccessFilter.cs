using DrugsApp.Domain.StudyModeFeature.Repositories;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DrugsApp.Features.StudyModeFeature.Shared;

/// <summary>
/// Action filter guarding every clinic-scoped study mode endpoint: short-circuits with 404 unless the route
/// <c>clinic_id</c> exists and is either public or private and created by the caller. Private clinics of other admin
/// users are reported as not found so their existence is not disclosed.
/// </summary>
public sealed class StudyClinicAccessFilter(IStudyClinicRepository _studyClinicRepository) : IAsyncActionFilter
{
    /// <summary>
    /// Verifies clinic visibility for the calling admin user before the action executes.
    /// </summary>
    /// <param name="context">
    /// Executing action context.
    /// </param>
    /// <param name="next">
    /// Delegate executing the action.
    /// </param>
    /// <returns>
    /// A task completing once the action ran or the request was short-circuited.
    /// </returns>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.User.TryGetUserId(out Guid callerUserId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        bool hasClinicId = Guid.TryParse(
            context.RouteData.Values[StudyPatientScopeFilter.ClinicIdRouteKey]?.ToString(),
            out Guid clinicId);

        bool isAccessible = hasClinicId
            && await _studyClinicRepository.IsAccessibleAsync(
                clinicId,
                callerUserId,
                context.HttpContext.RequestAborted);

        if (!isAccessible)
        {
            List<Error> errors = [StudyModeErrors.ClinicNotFound(clinicId)];
            context.Result = errors.MapToProblem(context.HttpContext);
            return;
        }

        await next();
    }
}
