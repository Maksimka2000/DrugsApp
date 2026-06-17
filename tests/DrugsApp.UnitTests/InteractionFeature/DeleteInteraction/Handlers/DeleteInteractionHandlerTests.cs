using DrugsApp.Domain.InteractionFeature.Entities;
using DrugsApp.Domain.InteractionFeature.Repositories;
using DrugsApp.Features.InteractionFeature.DeleteInteraction;
using ErrorOr;
using FluentAssertions;
using Moq;

namespace DrugsApp.UnitTests.InteractionFeature.DeleteInteraction.Handlers;

public sealed class DeleteInteractionHandlerTests
{
    [Fact]
    public async Task Handle_WhenInteractionNotFound_ReturnsNotFound()
    {
        var interactionId = Guid.NewGuid();
        var repository = new Mock<IInteractionRepository>();
        repository
            .Setup(r => r.GetByIdAsync(interactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Interaction?)null);

        var handler = new DeleteInteractionHandler(repository.Object);

        ErrorOr<Success> result = await handler.Handle(
            new DeleteInteractionCommand(interactionId),
            CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        result.FirstError.Description.Should().Be("Interaction not found.");
        repository.Verify(r => r.Remove(It.IsAny<Interaction>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenInteractionExists_RemovesAndReturnsSuccess()
    {
        InteractionSeverity severity = InteractionSeverity.Create(1, "High", null);
        Interaction interaction = Interaction.Create(severity.Id, "Default", null).Value;

        var repository = new Mock<IInteractionRepository>();
        repository
            .Setup(r => r.GetByIdAsync(interaction.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(interaction);

        var handler = new DeleteInteractionHandler(repository.Object);

        ErrorOr<Success> result = await handler.Handle(
            new DeleteInteractionCommand(interaction.Id),
            CancellationToken.None);

        result.IsError.Should().BeFalse();
        repository.Verify(r => r.Remove(interaction), Times.Once);
    }
}
