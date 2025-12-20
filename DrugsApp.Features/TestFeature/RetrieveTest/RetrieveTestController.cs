using System;
using DrugsApp.Features;
using DrugsApp.Features.TestFeature.Shared;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.TestFeature.RetrieveTest;

[ApiController]
[Tags("Test")]
[Route("api/tests")]
public sealed class RetrieveTestController : ControllerBase
{
    public const string RouteName = "GetTestById";

    private readonly IMediator _mediator;

    public RetrieveTestController(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{testId:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(TestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TestDto>> GetById(
        [FromRoute] Guid testId,
        CancellationToken cancellationToken)
    {
    
        GetTestByIdQuery query = RetrieveTestMapping.MapToQuery(testId);
        ErrorOr<TestDto> response = await _mediator.Send(query, cancellationToken);


        if (response.IsError)
        {
            return response.Errors.MapToProblem(HttpContext);
        }

        return Ok(response.Value);
    }
}

public sealed record GetTestByIdQuery(Guid TestId) : IRequest<ErrorOr<TestDto>>;

