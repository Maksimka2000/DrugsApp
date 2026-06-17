using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.ActiveSubstanceFeature.Repositories;
using DrugsApp.Domain.ActiveSubstanceGroupFeature.Repositories;
using DrugsApp.Domain.FoodFeature.Repositories;
using DrugsApp.Domain.InteractionFeature.Entities;
using DrugsApp.Domain.InteractionFeature.Enums;
using DrugsApp.Domain.InteractionFeature.Repositories;
using DrugsApp.Domain.SupplementFeature.Repositories;
using DrugsApp.Features.InteractionFeature.ApplicationServices;
using DrugsApp.Features.InteractionFeature.CreateInteraction;
using ErrorOr;
using FluentAssertions;
using Moq;

namespace DrugsApp.UnitTests.InteractionFeature.CreateInteraction.Handlers;

public sealed class CreateInteractionHandlerTests
{
    [Fact]
    public async Task Handle_WhenActiveSubstanceNotFound_ReturnsNotFound()
    {
        var missingId = Guid.NewGuid();
        var handler = BuildHandler(
            activeSubstanceRepository: MockActiveSubstanceRepo(missingId, substance: null));

        var command = new CreateInteractionCommand(new CreateInteractionRequest(
            Left: new InteractionSideRequest(InteractionType.Substance, missingId),
            Right: new InteractionSideRequest(InteractionType.Substance, Guid.NewGuid()),
            SeverityId: Guid.NewGuid(),
            DefaultText: "Interaction",
            StomachText: null));

        ErrorOr<CreatedInteractionResponse> result = await handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        result.FirstError.Description.Should().Be("Active substance not found.");
    }

    [Fact]
    public async Task Handle_WhenCanonicalPairAlreadyExists_ReturnsConflict()
    {
        var leftId = Guid.NewGuid();
        var rightId = Guid.NewGuid();
        ActiveSubstance left = ActiveSubstance.Create("Substance A");
        ActiveSubstance right = ActiveSubstance.Create("Substance B");
        InteractionSeverity severity = InteractionSeverity.Create(1, "Moderate", null);
        var existingInteraction = Interaction.Create(severity.Id, "Existing", null).Value;

        var interactionRepository = new Mock<IInteractionRepository>();
        interactionRepository
            .Setup(r => r.GetInteractionSeverityByIdAsync(severity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(severity);
        interactionRepository
            .Setup(r => r.GetByCanonicalPairAsync(It.IsAny<InteractionPartyKey>(), It.IsAny<InteractionPartyKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingInteraction);

        var handler = BuildHandler(
            interactionRepository: interactionRepository,
            activeSubstanceRepository: MockActiveSubstanceRepo(leftId, left, rightId, right));

        var command = new CreateInteractionCommand(new CreateInteractionRequest(
            Left: new InteractionSideRequest(InteractionType.Substance, leftId),
            Right: new InteractionSideRequest(InteractionType.Substance, rightId),
            SeverityId: severity.Id,
            DefaultText: "New interaction",
            StomachText: null));

        ErrorOr<CreatedInteractionResponse> result = await handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Conflict);
        result.FirstError.Description.Should().Be("Interaction already exists for the provided parties.");
    }

    [Fact]
    public async Task Handle_WhenSeverityNotFound_ReturnsNotFound()
    {
        var leftId = Guid.NewGuid();
        var rightId = Guid.NewGuid();
        var severityId = Guid.NewGuid();
        ActiveSubstance left = ActiveSubstance.Create("Substance A");
        ActiveSubstance right = ActiveSubstance.Create("Substance B");

        var interactionRepository = new Mock<IInteractionRepository>();
        interactionRepository
            .Setup(r => r.GetInteractionSeverityByIdAsync(severityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((InteractionSeverity?)null);

        var handler = BuildHandler(
            interactionRepository: interactionRepository,
            activeSubstanceRepository: MockActiveSubstanceRepo(leftId, left, rightId, right));

        var command = new CreateInteractionCommand(new CreateInteractionRequest(
            Left: new InteractionSideRequest(InteractionType.Substance, leftId),
            Right: new InteractionSideRequest(InteractionType.Substance, rightId),
            SeverityId: severityId,
            DefaultText: "Interaction",
            StomachText: null));

        ErrorOr<CreatedInteractionResponse> result = await handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        result.FirstError.Description.Should().Be("Interaction severity not found.");
    }

    private static CreateInteractionHandler BuildHandler(
        Mock<IInteractionRepository>? interactionRepository = null,
        Mock<IActiveSubstanceRepository>? activeSubstanceRepository = null,
        Mock<IInteractionWriteApplicationService>? writeService = null)
    {
        return new CreateInteractionHandler(
            (interactionRepository ?? new Mock<IInteractionRepository>()).Object,
            (activeSubstanceRepository ?? new Mock<IActiveSubstanceRepository>()).Object,
            new Mock<IActiveSubstanceGroupRepository>().Object,
            new Mock<IFoodRepository>().Object,
            new Mock<IFoodGroupRepository>().Object,
            new Mock<ISupplementRepository>().Object,
            new Mock<ISupplementGroupRepository>().Object,
            (writeService ?? new Mock<IInteractionWriteApplicationService>()).Object);
    }

    private static Mock<IActiveSubstanceRepository> MockActiveSubstanceRepo(
        Guid leftId,
        ActiveSubstance? substance,
        Guid? rightId = null,
        ActiveSubstance? rightSubstance = null)
    {
        var repository = new Mock<IActiveSubstanceRepository>();
        repository
            .Setup(r => r.GetByIdAsync(leftId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(substance);
        if (rightId is not null)
        {
            repository
                .Setup(r => r.GetByIdAsync(rightId.Value, It.IsAny<CancellationToken>()))
                .ReturnsAsync(rightSubstance);
        }

        return repository;
    }
}
