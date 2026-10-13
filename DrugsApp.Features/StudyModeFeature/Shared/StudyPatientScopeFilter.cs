using DrugsApp.Domain.StudyModeFeature.Enums;
using DrugsApp.Domain.StudyModeFeature.Repositories;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DrugsApp.Features.StudyModeFeature.Shared;

/// <summary>
/// Action filter guarding the study patient medical-data proxy controllers: short-circuits with 404 unless the
/// route <c>patient_id</c> is a patient member of the route <c>clinic_id</c>. It runs outside the MediatR pipeline,
/// so the proxied client command keeps exactly one transaction.
/// </summary>
public sealed class StudyPatientScopeFilter(IStudyClinicRepository _studyClinicRepository) : IAsyncActionFilter
{
    /// <summary>
    /// Route value name carrying the study clinic identifier.
    /// </summary>
    public const string ClinicIdRouteKey = "clinic_id";

    /// <summary>
    /// Route value name carrying the study patient user identifier.
    /// </summary>
    public const string PatientIdRouteKey = "patient_id";

    /// <summary>
    /// Verifies clinic membership of the route patient before the proxied action executes.
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
        bool hasClinicId = Guid.TryParse(context.RouteData.Values[ClinicIdRouteKey]?.ToString(), out Guid clinicId);
        bool hasPatientId = Guid.TryParse(context.RouteData.Values[PatientIdRouteKey]?.ToString(), out Guid patientUserId);

        bool isPatientMember = hasClinicId
            && hasPatientId
            && await _studyClinicRepository.ExistsMemberAsync(
                clinicId,
                patientUserId,
                StudyMemberKind.Patient,
                context.HttpContext.RequestAborted);

        if (!isPatientMember)
        {
            List<Error> errors = [StudyModeErrors.MemberNotFound(patientUserId, "patientId")];
            context.Result = errors.MapToProblem(context.HttpContext);
            return;
        }

        await next();
    }
}
