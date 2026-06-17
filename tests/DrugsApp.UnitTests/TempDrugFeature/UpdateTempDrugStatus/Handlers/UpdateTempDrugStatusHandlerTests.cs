using DrugsApp.Domain.TempDrugFeature.Entities;
using DrugsApp.Domain.TempDrugFeature.Repositories;
using DrugsApp.Features.TempDrugFeature.UpdateTempDrugStatus;
using ErrorOr;
using FluentAssertions;
using Moq;

namespace DrugsApp.UnitTests.TempDrugFeature.UpdateTempDrugStatus.Handlers;

public sealed class UpdateTempDrugStatusHandlerTests
{
    [Fact]
    public async Task Handle_WhenReviewedAddedRequested_ReturnsValidationError()
    {
        var handler = new UpdateTempDrugStatusHandler(new Mock<ITempDrugRepository>().Object);
        var command = new UpdateTempDrugStatusCommand(
            Guid.NewGuid(),
            new UpdateTempDrugStatusRequest(TempDrugStatus.ReviewedAdded, Note: null));

        ErrorOr<UpdatedTempDrugStatusResponse> result = await handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Description.Should().Contain("ReviewedAdded");
    }

    [Fact]
    public async Task Handle_WhenPendingReviewRequested_ReturnsValidationError()
    {
        var handler = new UpdateTempDrugStatusHandler(new Mock<ITempDrugRepository>().Object);
        var command = new UpdateTempDrugStatusCommand(
            Guid.NewGuid(),
            new UpdateTempDrugStatusRequest(TempDrugStatus.PendingReview, Note: null));

        ErrorOr<UpdatedTempDrugStatusResponse> result = await handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task Handle_WhenTempDrugNotFound_ReturnsNotFound()
    {
        var tempDrugId = Guid.NewGuid();
        var repository = new Mock<ITempDrugRepository>();
        repository
            .Setup(r => r.GetByIdAsync(tempDrugId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TempDrug?)null);

        var handler = new UpdateTempDrugStatusHandler(repository.Object);
        var command = new UpdateTempDrugStatusCommand(
            tempDrugId,
            new UpdateTempDrugStatusRequest(TempDrugStatus.ReviewLater, Note: null));

        ErrorOr<UpdatedTempDrugStatusResponse> result = await handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        result.FirstError.Metadata!["resourceType"].Should().Be("TempDrug");
    }

    [Fact]
    public async Task Handle_WhenReviewLaterRequested_UpdatesStatusAndNote()
    {
        TempDrug tempDrug = TempDrug.CreateImported(
            productIdentifier: "500",
            commercialName: "Apap",
            activeSubstance: "Paracetamol",
            typeOfProduct: "Ludzki",
            routeOfAdministration: null,
            pharmaceuticalFormulation: null,
            atcCode: null,
            leaflet: null,
            educationalMaterials: null,
            dosage: null,
            linkedImportId: Guid.NewGuid()).Value;

        var repository = new Mock<ITempDrugRepository>();
        repository
            .Setup(r => r.GetByIdAsync(tempDrug.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tempDrug);

        var handler = new UpdateTempDrugStatusHandler(repository.Object);
        var command = new UpdateTempDrugStatusCommand(
            tempDrug.Id,
            new UpdateTempDrugStatusRequest(TempDrugStatus.ReviewLater, Note: "  defer  "));

        ErrorOr<UpdatedTempDrugStatusResponse> result = await handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Status.Should().Be(TempDrugStatus.ReviewLater);
        result.Value.Note.Should().Be("defer");
        tempDrug.Status.Should().Be(TempDrugStatus.ReviewLater);
    }

    [Fact]
    public async Task Handle_WhenReviewedResolvedRequested_UpdatesStatus()
    {
        TempDrug tempDrug = TempDrug.CreateImported(
            productIdentifier: "501",
            commercialName: "Apap",
            activeSubstance: "Paracetamol",
            typeOfProduct: "Ludzki",
            routeOfAdministration: null,
            pharmaceuticalFormulation: null,
            atcCode: null,
            leaflet: null,
            educationalMaterials: null,
            dosage: null,
            linkedImportId: Guid.NewGuid()).Value;

        var repository = new Mock<ITempDrugRepository>();
        repository
            .Setup(r => r.GetByIdAsync(tempDrug.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tempDrug);

        var handler = new UpdateTempDrugStatusHandler(repository.Object);
        var command = new UpdateTempDrugStatusCommand(
            tempDrug.Id,
            new UpdateTempDrugStatusRequest(TempDrugStatus.ReviewedResolved, Note: null));

        ErrorOr<UpdatedTempDrugStatusResponse> result = await handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Status.Should().Be(TempDrugStatus.ReviewedResolved);
        result.Value.Note.Should().BeEmpty();
    }
}
