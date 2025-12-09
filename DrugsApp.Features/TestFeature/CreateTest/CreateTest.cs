using DrugsApp.Domain.TestFeature.Entities;
using DrugsApp.Domain.TestFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.TestFeature.CreateTest;

internal sealed class CreateTestHandler
    : IRequestHandler<CreateTestCommand, ErrorOr<CreatedTestResponse>>
{
    private readonly ITestRepository _testRepository;

    public CreateTestHandler(ITestRepository testRepository)
    {
        _testRepository = testRepository;
    }

    public async Task<ErrorOr<CreatedTestResponse>> Handle(
        CreateTestCommand command,
        CancellationToken cancellationToken)
    {
        Test testEntity = command.MapToEntity();

        await _testRepository.AddAsync(testEntity, cancellationToken);

        CreatedTestResponse response = testEntity.MapToResponse();

        return response;
    }
}

