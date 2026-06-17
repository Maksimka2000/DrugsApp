using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.TempDrugFeature.Entities;
using DrugsApp.Features.DrugFeature.Shared;
using DrugsApp.Features.TempDrugFeature.ApplicationServices;
using DrugsApp.Features.TempDrugFeature.CreateDrugFromTempDrugReview;
using ErrorOr;
using FluentAssertions;
using Moq;

namespace DrugsApp.UnitTests.TempDrugFeature.CreateDrugFromTempDrugReview.Handlers;

public sealed class CreateDrugFromTempDrugReviewHandlerTests
{
    [Fact]
    public async Task Handle_WhenApplicationServiceReturnsError_PropagatesError()
    {
        var reviewService = new Mock<ITempDrugReviewApplicationService>();
        reviewService
            .Setup(s => s.CreateDrugAsync(It.IsAny<CreateDrugFromTempDrugReviewTempDrugApplicationServiceDraft>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Conflict(description: "TempDrug is already in a terminal status."));

        var handler = new CreateDrugFromTempDrugReviewHandler(reviewService.Object);
        var command = new CreateDrugFromTempDrugReviewCommand(
            Guid.NewGuid(),
            new CreateDrugFromTempDrugReviewRequest(
                Note: null,
                CommercialName: "Apap",
                TypeOfProduct: "Ludzki",
                TypeOfDrug: DrugType.Standard,
                ActiveSubstanceIds: [Guid.NewGuid()],
                ProductIdentifier: "12345",
                AtcCode: null,
                Dosage: null,
                Leaflet: null,
                EducationalMaterials: null,
                RouteOfAdministrationIds: null,
                PharmaceuticalFormIds: null));

        ErrorOr<CreatedDrugFromTempDrugReviewResponse> result =
            await handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task Handle_WhenApplicationServiceSucceeds_ReturnsMappedResponse()
    {
        var tempDrugId = Guid.NewGuid();
        var linkedDrugFormId = Guid.NewGuid();
        DrugDto drug = new(
            Id: Guid.NewGuid(),
            CommercialName: "Apap",
            TypeOfProduct: "Ludzki",
            TypeOfDrug: DrugType.Standard,
            DrugFormsCount: 1,
            ActiveSubstances: [],
            DrugForms: []);

        var reviewService = new Mock<ITempDrugReviewApplicationService>();
        reviewService
            .Setup(s => s.CreateDrugAsync(It.IsAny<CreateDrugFromTempDrugReviewTempDrugApplicationServiceDraft>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CreateDrugFromTempDrugReviewTempDrugApplicationServiceResult(
                TempDrugId: tempDrugId,
                NewStatus: TempDrugStatus.ReviewedAdded,
                LinkedDrugFormId: linkedDrugFormId,
                Note: "note",
                Drug: drug));

        var handler = new CreateDrugFromTempDrugReviewHandler(reviewService.Object);
        var command = new CreateDrugFromTempDrugReviewCommand(
            tempDrugId,
            new CreateDrugFromTempDrugReviewRequest(
                Note: "  note  ",
                CommercialName: " Apap ",
                TypeOfProduct: " Ludzki ",
                TypeOfDrug: DrugType.Standard,
                ActiveSubstanceIds: [Guid.NewGuid()],
                ProductIdentifier: " 12345 ",
                AtcCode: null,
                Dosage: null,
                Leaflet: null,
                EducationalMaterials: null,
                RouteOfAdministrationIds: null,
                PharmaceuticalFormIds: null));

        ErrorOr<CreatedDrugFromTempDrugReviewResponse> result =
            await handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.TempDrugId.Should().Be(tempDrugId);
        result.Value.NewStatus.Should().Be(TempDrugStatus.ReviewedAdded);
        result.Value.LinkedDrugFormId.Should().Be(linkedDrugFormId);
        result.Value.Drug.CommercialName.Should().Be("Apap");
    }
}
