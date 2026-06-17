using DrugsApp.Domain.InteractionFeature.Entities;
using DrugsApp.Domain.InteractionFeature.Repositories;
using DrugsApp.Features.InteractionFeature.UpdateInteraction;
using ErrorOr;
using FluentAssertions;
using Moq;

namespace DrugsApp.UnitTests.InteractionFeature.UpdateInteraction.Handlers;

public sealed class UpdateInteractionHandlerTests
{
    [Fact]
    public async Task Handle_WhenInteractionNotFound_ReturnsNotFound()
    {
        var interactionId = Guid.NewGuid();
        var repository = new Mock<IInteractionRepository>();
        repository
            .Setup(r => r.GetByIdAsync(interactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Interaction?)null);

        var handler = new UpdateInteractionHandler(repository.Object);
        var command = new UpdateInteractionCommand(
            interactionId,
            new UpdateInteractionRequest(
                DefaultText: "Updated",
                StomachText: null,
                SeverityId: null,
                ClearDefaultText: null,
                ClearStomachText: null));

        ErrorOr<UpdatedInteractionResponse> result = await handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        result.FirstError.Description.Should().Be("Interaction not found.");
    }

    [Fact]
    public async Task Handle_WhenClearingBothTexts_ReturnsValidationError()
    {
        InteractionSeverity severity = InteractionSeverity.Create(1, "High", null);
        Interaction interaction = Interaction.Create(severity.Id, "Default", "Stomach").Value;
        interaction.UpdateSeverity(severity.Id);

        var repository = new Mock<IInteractionRepository>();
        repository
            .Setup(r => r.GetByIdAsync(interaction.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(interaction);

        var handler = new UpdateInteractionHandler(repository.Object);
        var command = new UpdateInteractionCommand(
            interaction.Id,
            new UpdateInteractionRequest(
                DefaultText: null,
                StomachText: null,
                SeverityId: null,
                ClearDefaultText: true,
                ClearStomachText: true));

        ErrorOr<UpdatedInteractionResponse> result = await handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Description.Should().Be("At least one interaction text is required.");
    }

    [Fact]
    public async Task Handle_WhenSeverityNotFound_ReturnsNotFound()
    {
        var severityId = Guid.NewGuid();
        InteractionSeverity existingSeverity = InteractionSeverity.Create(1, "Moderate", null);
        Interaction interaction = Interaction.Create(existingSeverity.Id, "Default", null).Value;
        interaction.UpdateSeverity(existingSeverity.Id);

        var repository = new Mock<IInteractionRepository>();
        repository
            .Setup(r => r.GetByIdAsync(interaction.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(interaction);
        repository
            .Setup(r => r.GetInteractionSeverityByIdAsync(severityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((InteractionSeverity?)null);

        var handler = new UpdateInteractionHandler(repository.Object);
        var command = new UpdateInteractionCommand(
            interaction.Id,
            new UpdateInteractionRequest(
                DefaultText: null,
                StomachText: null,
                SeverityId: severityId,
                ClearDefaultText: null,
                ClearStomachText: null));

        ErrorOr<UpdatedInteractionResponse> result = await handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        result.FirstError.Description.Should().Be("Interaction severity not found.");
    }

    [Fact]
    public async Task Handle_WhenUpdatingDefaultText_ReturnsUpdatedResponse()
    {
        InteractionSeverity severity = InteractionSeverity.Create(2, "Low", "Minor");
        Interaction interaction = Interaction.Create(severity.Id, "Old default", "Stomach").Value;

        var repository = new Mock<IInteractionRepository>();
        repository
            .Setup(r => r.GetByIdAsync(interaction.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(interaction);
        repository
            .Setup(r => r.GetInteractionSeverityByIdAsync(severity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(severity);

        var handler = new UpdateInteractionHandler(repository.Object);
        var command = new UpdateInteractionCommand(
            interaction.Id,
            new UpdateInteractionRequest(
                DefaultText: "New default",
                StomachText: null,
                SeverityId: severity.Id,
                ClearDefaultText: null,
                ClearStomachText: null));

        ErrorOr<UpdatedInteractionResponse> result = await handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.DefaultText.Should().Be("New default");
        result.Value.StomachText.Should().Be("Stomach");
        result.Value.Severity.SeverityNumber.Should().Be(2);
        result.Value.Severity.Name.Should().Be("Low");
    }
}
