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

namespace DrugsApp.Features.UserProfileFeature.UpdateHealthcareProviderProfile;

[ApiController]
[Route("api/healthcare-provider/profile")]
[Authorize(Roles = "Client,HealthcareProvider")]
[Tags("User Profile")]
public sealed class UpdateHealthcareProviderProfileController(
    IMediator _mediator,
    IValidator<UpdateHealthcareProviderProfileRequest> _validator) : ControllerBase
{
    public const string RouteName = "UpdateHealthcareProviderProfile";

    /// <summary>
    /// Updates the healthcare provider profile for the current user.
    /// </summary>
    [HttpPut(Name = RouteName)]
    [ProducesResponseType(typeof(HealthcareProviderProfileResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<HealthcareProviderProfileResponse>> Update(
        [FromBody] UpdateHealthcareProviderProfileRequest request,
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

        UpdateHealthcareProviderProfileCommand command = request.MapToCommand(userId);

        ErrorOr<HealthcareProviderProfileResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
