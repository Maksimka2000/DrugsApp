using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.UserProfileFeature.Shared;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using FluentValidation.Results;

namespace DrugsApp.Features.UserProfileFeature.UploadUserProfilePhoto;

[ApiController]
[Route("api/user-profile/photo")]
[Authorize(Roles = "Client,HealthcareProvider")]
[Tags("User Profile")]
public sealed class UploadUserProfilePhotoController(
    IMediator _mediator,
    IValidator<UploadUserProfilePhotoRequest> _validator) : ControllerBase
{
    public const string RouteName = "UploadUserProfilePhoto";

    /// <summary>
    /// Uploads a profile photo for the current user.
    /// </summary>
    [HttpPost(Name = RouteName)]
    [ProducesResponseType(typeof(UserProfilePhotoResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserProfilePhotoResponse>> Upload(
        [FromForm] UploadUserProfilePhotoRequest request,
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

        UploadUserProfilePhotoCommand command = new(userId, request.File);

        ErrorOr<UserProfilePhotoResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
