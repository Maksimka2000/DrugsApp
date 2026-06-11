using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.DrugToolFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.DrugToolFeature.GetDrugToolDetail;

public sealed record GetDrugToolDetailRequest(
    Guid? SelectedDrugFormId,
    string InteractionScope = "all");

public sealed record GetDrugToolDetailQuery(
    Guid DrugId,
    Guid? SelectedDrugFormId,
    DrugToolInteractionScope InteractionScope,
    Guid UserId) : IRequest<ErrorOr<GetDrugToolDetailResponse>>;

[ApiController]
[Authorize(Roles = AuthRoles.MemberRoles)]
[Tags("Drug Tool")]
[Route("api/tools/drugs")]
public sealed class GetDrugToolDetailController(
    IMediator _mediator,
    IValidator<GetDrugToolDetailRequest> _validator) : ControllerBase
{
    public const string RouteName = "GetDrugToolDetail";

    [HttpGet("{drug_id:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(GetDrugToolDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetDrugToolDetailResponse>> Get(
        [FromRoute(Name = "drug_id")] Guid drugId,
        [FromQuery(Name = "selected_drug_form_id")] Guid? selectedDrugFormId,
        [FromQuery(Name = "interaction_scope")] string? interactionScope,
        CancellationToken cancellationToken)
    {
        GetDrugToolDetailRequest request = new(
            selectedDrugFormId,
            string.IsNullOrWhiteSpace(interactionScope) ? "all" : interactionScope);

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

        GetDrugToolDetailQuery query = GetDrugToolDetailMapping.MapToQuery(
            drugId,
            request.SelectedDrugFormId,
            request.InteractionScope,
            userId);

        ErrorOr<GetDrugToolDetailResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
