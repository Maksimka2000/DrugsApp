using System;
using DrugsApp.Features;
using DrugsApp.Features.TestFeature.RetrieveTest;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.TestFeature.CreateTest;

[ApiController]
[Route("api/tests")]
public sealed class CreateTestController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<CreateTestRequest> _validator;

    public CreateTestController(
        IMediator mediator,
        IValidator<CreateTestRequest> validator)
    {
        _mediator = mediator;
        _validator = validator;
    }

    [HttpPost]
    public async Task<ActionResult<CreatedTestResponse>> Create(
        [FromBody] CreateTestRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult? validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        CreateTestCommand command = request.MapToCommand();
        ErrorOr<CreatedTestResponse> response = await _mediator.Send(command, cancellationToken);


        return response.Match(
               respnonse => CreatedAtRoute(
                    routeName: RetrieveTestController.RouteName,
                    routeValues: new { testId = response.Value.Id },
                    value: response.Value),
               errors => errors.MapToProblem(HttpContext));
            
            
    }
}

public sealed record CreateTestRequest
{
    public string Attribute1 { get; init; } = string.Empty;
    public int? Attribute2 { get; init; }
    public decimal? Attribute3 { get; init; }
    public bool? Attribute4 { get; init; }
}

public sealed record CreatedTestResponse(
    Guid Id,
    string Attribute1,
    int? Attribute2,
    decimal? Attribute3,
    bool Attribute4);

public sealed record CreateTestCommand(
    string Attribute1,
    int? Attribute2,
    decimal? Attribute3,
    bool? Attribute4) : IRequest<ErrorOr<CreatedTestResponse>>;

