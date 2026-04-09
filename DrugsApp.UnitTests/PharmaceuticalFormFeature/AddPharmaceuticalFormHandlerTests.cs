using DrugsApp.Domain.PharmaceuticalFormFeature.Entities;
using DrugsApp.Domain.PharmaceuticalFormFeature.Repositories;
using DrugsApp.Features.PharmaceuticalFormFeature.AddPharmaceuticalForm;
using ErrorOr;
using Moq;

namespace DrugsApp.UnitTests.PharmaceuticalFormFeature;

public sealed class AddPharmaceuticalFormHandlerTests
{
    [Fact]
    public async Task Handle_Success_WhenUnique_PersistsAndReturnsCreated()
    {
        Mock<IPharmaceuticalFormRepository> repository = new(MockBehavior.Strict);
        PharmaceuticalForm? added = null;

        repository
            .Setup(r => r.GetByNameAsync("Tablet", It.IsAny<CancellationToken>()))
            .ReturnsAsync((PharmaceuticalForm?)null);

        repository
            .Setup(r => r.AddAsync(It.IsAny<PharmaceuticalForm>(), It.IsAny<CancellationToken>()))
            .Callback<PharmaceuticalForm, CancellationToken>((entity, _) => added = entity)
            .Returns(Task.CompletedTask);

        AddPharmaceuticalFormHandler handler = new(repository.Object);
        AddPharmaceuticalFormCommand command = new(new AddPharmaceuticalFormRequest("  Tablet  "));

        ErrorOr<CreatedPharmaceuticalFormResponse> result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.NotNull(added);
        Assert.Equal("Tablet", added!.FormulationName);
        Assert.Equal(added.Id, result.Value.Id);
    }

    [Fact]
    public async Task Handle_Failure_WhenFormExists_ReturnsConflict()
    {
        Mock<IPharmaceuticalFormRepository> repository = new(MockBehavior.Strict);
        var existing = PharmaceuticalForm.Create("Tablet");

        repository
            .Setup(r => r.GetByNameAsync("Tablet", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        AddPharmaceuticalFormHandler handler = new(repository.Object);
        AddPharmaceuticalFormCommand command = new(new AddPharmaceuticalFormRequest("Tablet"));

        ErrorOr<CreatedPharmaceuticalFormResponse> result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal(ErrorType.Conflict, result.FirstError.Type);
    }

    [Fact]
    public async Task Handle_Edge_TrimmedNameCollidesWithExistingNormalizedName_ReturnsConflict()
    {
        Mock<IPharmaceuticalFormRepository> repository = new(MockBehavior.Strict);
        var existing = PharmaceuticalForm.Create("Tablet");

        repository
            .Setup(r => r.GetByNameAsync("Tablet", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        AddPharmaceuticalFormHandler handler = new(repository.Object);
        AddPharmaceuticalFormCommand command = new(new AddPharmaceuticalFormRequest("  Tablet  "));

        ErrorOr<CreatedPharmaceuticalFormResponse> result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal(ErrorType.Conflict, result.FirstError.Type);
    }
}
