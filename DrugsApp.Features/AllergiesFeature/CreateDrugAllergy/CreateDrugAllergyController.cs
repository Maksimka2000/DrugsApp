using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.AllergiesFeature.Shared;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.AllergiesFeature.CreateDrugAllergy;

/// <summary>
/// Request model for creating a drug allergy.
/// </summary>
public sealed record CreateDrugAllergyRequest(
    Guid? DrugFormId,
    string? CustomDrugName,
    string? CustomActiveSubstances,
    string? CustomFormulationName,
    string? CustomDosage,
    string? Note,
    string Source);

/// <summary>
/// Command for creating a drug allergy.
/// </summary>
public sealed record CreateDrugAllergyCommand(
    Guid UserId,
    CreateDrugAllergyRequest Request) : IRequest<ErrorOr<CreatedDrugAllergyResponse>>;

/// <summary>
/// Controller for creating drug allergies.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.MemberRoles)]
[Tags("Allergies")]
[Route("api/allergies")]
public sealed class CreateDrugAllergyController(
    IMediator _mediator,
    IValidator<CreateDrugAllergyRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name for creating a drug allergy.
    /// </summary>
    public const string RouteName = "CreateDrugAllergy";

    /// <summary>
    /// Creates a drug allergy for the current user.
    /// </summary>
    [HttpPost("drugs", Name = RouteName)]
    [ProducesResponseType(typeof(CreatedDrugAllergyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreatedDrugAllergyResponse>> Create(
        [FromBody] CreateDrugAllergyRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId))
        {
            return Unauthorized();
        }

        CreateDrugAllergyCommand command = request.MapToCommand(userId);
        ErrorOr<CreatedDrugAllergyResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<CreatedDrugAllergyResponse>>(
            response => CreatedAtRoute(
                routeName: UpdateDrugAllergyNote.UpdateDrugAllergyNoteController.RouteName,
                routeValues: new { id = response.Item.Id },
                value: response),
            errors => errors.MapToProblem(HttpContext));
    }
}
