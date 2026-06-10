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

namespace DrugsApp.Features.UserProfileFeature.AddHealthcareProviderProfile;

[ApiController]
[Route("api/healthcare-provider/profile")]
[Authorize(Roles = "Client,HealthcareProvider")]
[Tags("User Profile")]
public sealed class AddHealthcareProviderProfileController(
    IMediator _mediator,
    IValidator<AddHealthcareProviderProfileRequest> _validator) : ControllerBase
{
    public const string RouteName = "AddHealthcareProviderProfile";

    /// <summary>
    /// Creates a healthcare provider profile for the current user.
    /// </summary>
    [HttpPost(Name = RouteName)]
    [ProducesResponseType(typeof(HealthcareProviderProfileResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<HealthcareProviderProfileResponse>> Add(
        [FromBody] AddHealthcareProviderProfileRequest request,
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

        AddHealthcareProviderProfileCommand command = request.MapToCommand(userId);

        ErrorOr<HealthcareProviderProfileResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<HealthcareProviderProfileResponse>>(
            response => CreatedAtRoute(UpdateHealthcareProviderProfile.UpdateHealthcareProviderProfileController.RouteName, new { }, response),
            errors => errors.MapToProblem(HttpContext));
    }
}
