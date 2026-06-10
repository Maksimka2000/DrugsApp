using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.UserProfileFeature.Shared;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;

namespace DrugsApp.Features.UserProfileFeature.UpdateReproductiveHealthProfile;

[ApiController]
[Route("api/user-profile/reproductive-health")]
[Authorize(Roles = "Client,HealthcareProvider")]
[Tags("User Profile")]
public sealed class UpdateReproductiveHealthProfileController(
    IMediator _mediator,
    IValidator<UpdateReproductiveHealthProfileRequest> _validator) : ControllerBase
{
    public const string RouteName = "UpdateReproductiveHealthProfile";

    /// <summary>
    /// Upserts the reproductive health profile for the current user.
    /// </summary>
    [HttpPut(Name = RouteName)]
    [ProducesResponseType(typeof(ReproductiveHealthProfileResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReproductiveHealthProfileResponse>> Update(
        [FromBody] UpdateReproductiveHealthProfileRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult? validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId))
        {
            return Unauthorized();
        }

        UpdateReproductiveHealthProfileCommand command = request.MapToCommand(userId);

        ErrorOr<ReproductiveHealthProfileResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
