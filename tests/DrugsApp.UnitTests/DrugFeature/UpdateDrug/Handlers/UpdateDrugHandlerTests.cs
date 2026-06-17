using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.ActiveSubstanceFeature.Repositories;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.DrugFeature.Repositories;
using DrugsApp.Features.DrugFeature.UpdateDrug;
using ErrorOr;
using FluentAssertions;
using Moq;

namespace DrugsApp.UnitTests.DrugFeature.UpdateDrug.Handlers;

public sealed class UpdateDrugHandlerTests
{
    [Fact]
    public async Task Handle_WhenDrugNotFound_ReturnsNotFound()
    {
        var drugId = Guid.NewGuid();
        var drugRepository = new Mock<IDrugRepository>();
        drugRepository
            .Setup(r => r.GetByIdAsync(drugId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Drug?)null);

        var handler = new UpdateDrugHandler(
            drugRepository.Object,
            new Mock<IActiveSubstanceRepository>().Object);

        var command = new UpdateDrugCommand(
            drugId,
            new UpdateDrugRequest(CommercialName: "Updated", NewActiveSubstanceIds: null, TypeOfDrug: null));

        ErrorOr<UpdatedDrugResponse> result = await handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        result.FirstError.Metadata!["resourceType"].Should().Be("Drug");
    }

    [Fact]
    public async Task Handle_WhenMissingActiveSubstanceIds_ReturnsValidationError()
    {
        var drugId = Guid.NewGuid();
        var existingSubstance = ActiveSubstance.Create("Paracetamol");
        var missingSubstanceId = Guid.NewGuid();

        Drug drug = Drug.Create(
            "Apap",
            "Ludzki",
            DrugType.Standard,
            [existingSubstance],
            drugForms: null);

        var drugRepository = new Mock<IDrugRepository>();
        drugRepository
            .Setup(r => r.GetByIdAsync(drugId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(drug);

        var activeSubstanceRepository = new Mock<IActiveSubstanceRepository>();
        activeSubstanceRepository
            .Setup(r => r.GetByIdsForLinkingAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new UpdateDrugHandler(
            drugRepository.Object,
            activeSubstanceRepository.Object);

        var command = new UpdateDrugCommand(
            drugId,
            new UpdateDrugRequest(
                CommercialName: null,
                NewActiveSubstanceIds: [missingSubstanceId],
                TypeOfDrug: null));

        ErrorOr<UpdatedDrugResponse> result = await handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Description.Should().Be("One or more active substances were not found.");
    }

    [Fact]
    public async Task Handle_WhenEmptyNewActiveSubstanceIdsWithSingleSubstance_KeepsSubstance()
    {
        var drugId = Guid.NewGuid();
        ActiveSubstance existingSubstance = ActiveSubstance.Create("Paracetamol");
        ActiveSubstance secondSubstance = ActiveSubstance.Create("Ibuprofen");

        Drug drug = Drug.Create(
            "Apap",
            "Ludzki",
            DrugType.Standard,
            [existingSubstance, secondSubstance],
            drugForms: null);

        var drugRepository = new Mock<IDrugRepository>();
        drugRepository
            .Setup(r => r.GetByIdAsync(drugId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(drug);

        var handler = new UpdateDrugHandler(
            drugRepository.Object,
            new Mock<IActiveSubstanceRepository>().Object);

        var command = new UpdateDrugCommand(
            drugId,
            new UpdateDrugRequest(
                CommercialName: null,
                NewActiveSubstanceIds: [existingSubstance.Id],
                TypeOfDrug: null));

        ErrorOr<UpdatedDrugResponse> result = await handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.ActiveSubstances.Should().ContainSingle(s => s.Id == existingSubstance.Id);
    }

    [Fact]
    public async Task Handle_WhenUpdatingCommercialName_ReturnsUpdatedResponse()
    {
        var drugId = Guid.NewGuid();
        ActiveSubstance substance = ActiveSubstance.Create("Paracetamol");

        Drug drug = Drug.Create(
            "Apap",
            "Ludzki",
            DrugType.Standard,
            [substance],
            drugForms: null);

        var drugRepository = new Mock<IDrugRepository>();
        drugRepository
            .Setup(r => r.GetByIdAsync(drugId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(drug);

        var handler = new UpdateDrugHandler(
            drugRepository.Object,
            new Mock<IActiveSubstanceRepository>().Object);

        var command = new UpdateDrugCommand(
            drugId,
            new UpdateDrugRequest(
                CommercialName: "  New Name  ",
                NewActiveSubstanceIds: null,
                TypeOfDrug: null));

        ErrorOr<UpdatedDrugResponse> result = await handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.CommercialName.Should().Be("New Name");
    }
}
