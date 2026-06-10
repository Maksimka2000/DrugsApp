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

namespace DrugsApp.Features.UserProfileFeature.AddReproductiveHealthProfile;

[ApiController]
[Route("api/user-profile/reproductive-health")]
[Authorize(Roles = "Client,HealthcareProvider")]
[Tags("User Profile")]
public sealed class AddReproductiveHealthProfileController(
    IMediator _mediator,
    IValidator<AddReproductiveHealthProfileRequest> _validator) : ControllerBase
{
    public const string RouteName = "AddReproductiveHealthProfile";

    /// <summary>
    /// Adds a reproductive health profile for the current user (female only).
    /// </summary>
    [HttpPost(Name = RouteName)]
    [ProducesResponseType(typeof(ReproductiveHealthProfileResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<ReproductiveHealthProfileResponse>> Add(
        [FromBody] AddReproductiveHealthProfileRequest request,
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

        AddReproductiveHealthProfileCommand command = request.MapToCommand(userId);

        ErrorOr<ReproductiveHealthProfileResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<ReproductiveHealthProfileResponse>>(
            response => CreatedAtRoute(GetUserProfile.GetUserProfileController.RouteName, new { }, response),
            errors => errors.MapToProblem(HttpContext));
    }
}
