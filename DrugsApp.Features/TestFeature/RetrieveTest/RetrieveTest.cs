using System.Collections.Generic;
using DrugsApp.Domain.TestFeature.Entities;
using DrugsApp.Domain.TestFeature.Repositories;
using DrugsApp.Features.TestFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.TestFeature.RetrieveTest;

internal sealed class GetTestByIdHandler
    : IRequestHandler<GetTestByIdQuery, ErrorOr<TestDto>>
{
    private readonly ITestRepository _testRepository;

    public GetTestByIdHandler(ITestRepository testRepository)
    {
        _testRepository = testRepository;
    }

    public async Task<ErrorOr<TestDto>> Handle(
        GetTestByIdQuery query,
        CancellationToken cancellationToken)
    {
        Test? test = await _testRepository.GetByIdAsync(query.TestId, cancellationToken);

        if (test is null)
        {
            return Error.NotFound(
                description: "Test not found.",
                metadata: new Dictionary<string, object>
                {
                    ["resourceType"] = "test",
                    ["resourceId"] = query.TestId.ToString()
                });
        }

        TestDto response = test.MapToDto();

        return response;
    }
}

