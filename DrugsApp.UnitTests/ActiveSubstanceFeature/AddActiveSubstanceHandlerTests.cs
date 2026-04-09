using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.ActiveSubstanceFeature.Repositories;
using DrugsApp.Features.ActiveSubstanceFeature.AddActiveSubstance;
using ErrorOr;
using Moq;

namespace DrugsApp.UnitTests.ActiveSubstanceFeature;

/// <summary>
/// Tests <see cref="AddActiveSubstanceHandler"/> in isolation (repository is a mocked port).
/// </summary>
public sealed class AddActiveSubstanceHandlerTests
{
    [Fact]
    public async Task Handle_Success_WhenNameIsUnique_PersistsAndReturnsCreatedResponse()
    {
        Mock<IActiveSubstanceRepository> repository = new(MockBehavior.Strict);
        ActiveSubstance? added = null;

        repository
            .Setup(r => r.GetByNameAsync("Ibuprofen", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ActiveSubstance?)null);

        repository
            .Setup(r => r.AddAsync(It.IsAny<ActiveSubstance>(), It.IsAny<CancellationToken>()))
            .Callback<ActiveSubstance, CancellationToken>((entity, _) => added = entity)
            .Returns(Task.CompletedTask);

        AddActiveSubstanceHandler handler = new(repository.Object);
        AddActiveSubstanceCommand command = new(new AddActiveSubstanceRequest("  Ibuprofen  ", null));

        ErrorOr<CreatedActiveSubstanceResponse> result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.NotNull(added);
        Assert.Equal("Ibuprofen", added!.ActiveSubstanceName);
        Assert.Equal(added.Id, result.Value.Id);
        repository.Verify(r => r.AddAsync(It.IsAny<ActiveSubstance>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Failure_WhenNameAlreadyExists_ReturnsConflict()
    {
        Mock<IActiveSubstanceRepository> repository = new(MockBehavior.Strict);
        var existing = ActiveSubstance.Create("Ibuprofen");

        repository
            .Setup(r => r.GetByNameAsync("Ibuprofen", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        AddActiveSubstanceHandler handler = new(repository.Object);
        AddActiveSubstanceCommand command = new(new AddActiveSubstanceRequest("Ibuprofen", null));

        ErrorOr<CreatedActiveSubstanceResponse> result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal(ErrorType.Conflict, result.FirstError.Type);
        repository.Verify(r => r.AddAsync(It.IsAny<ActiveSubstance>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Edge_TrimmedRequestNameMatchesExistingNormalizedName_ReturnsConflict()
    {
        Mock<IActiveSubstanceRepository> repository = new(MockBehavior.Strict);
        var existing = ActiveSubstance.Create("Ibuprofen");

        repository
            .Setup(r => r.GetByNameAsync("Ibuprofen", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        AddActiveSubstanceHandler handler = new(repository.Object);
        AddActiveSubstanceCommand command = new(new AddActiveSubstanceRequest("  Ibuprofen  ", null));

        ErrorOr<CreatedActiveSubstanceResponse> result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal(ErrorType.Conflict, result.FirstError.Type);
        repository.Verify(r => r.GetByNameAsync("Ibuprofen", It.IsAny<CancellationToken>()), Times.Once);
    }
}
