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
using DrugsApp.Domain.AuthFeature;

namespace DrugsApp.Features.UserProfileFeature.UpdateUserProfile;

[ApiController]
[Route("api/user-profile")]
[Authorize(Roles = AuthRoles.MemberRoles)]
[Tags("User Profile")]
public sealed class UpdateUserProfileController(
    IMediator _mediator,
    IValidator<UpdateUserProfileRequest> _validator) : ControllerBase
{
    public const string RouteName = "UpdateUserProfile";

    /// <summary>
    /// Updates personal data for the current user.
    /// </summary>
    [HttpPut(Name = RouteName)]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserProfileResponse>> Update(
        [FromBody] UpdateUserProfileRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult? validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        string? email = User.FindFirstValue(ClaimTypes.Email);

        if (!Guid.TryParse(subject, out Guid userId) || string.IsNullOrWhiteSpace(email))
        {
            return Unauthorized();
        }

        UpdateUserProfileCommand command = request.MapToCommand(userId, email);

        ErrorOr<UserProfileResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
