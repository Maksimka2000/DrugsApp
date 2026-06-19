using DrugsApp.Features.DrugFeature.Shared;
using DrugsApp.Features.TempDrugFeature.ApplicationServices;
using DrugsApp.Features.TempDrugFeature.UpdateDrugFormLinksFromTempDrugReview;
using DrugsApp.Domain.TempDrugFeature.Entities;
using ErrorOr;
using FluentAssertions;
using Moq;

namespace DrugsApp.UnitTests.TempDrugFeature.UpdateDrugFormLinksFromTempDrugReview.Handlers;

public sealed class UpdateDrugFormLinksFromTempDrugReviewHandlerTests
{
    [Fact]
    public async Task Handle_WhenApplicationServiceReturnsError_PropagatesError()
    {
        var reviewService = new Mock<ITempDrugReviewApplicationService>();
        reviewService
            .Setup(s => s.UpdateDrugFormLinksAsync(It.IsAny<UpdateDrugFormLinksFromTempDrugReviewTempDrugApplicationServiceDraft>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Conflict(description: "TempDrug is already linked to a canonical record."));

        var handler = new UpdateDrugFormLinksFromTempDrugReviewHandler(reviewService.Object);

        ErrorOr<UpdatedDrugFormLinksFromTempDrugReviewResponse> result =
            await handler.Handle(SampleCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task Handle_WhenApplicationServiceSucceeds_ReturnsMappedResponse()
    {
        var tempDrugId = Guid.NewGuid();
        var drugFormId = Guid.NewGuid();
        DrugFormDto drugForm = new(
            Id: drugFormId,
            ProductIdentifier: 100,
            AtcCode: null,
            Dosage: null,
            Leaflet: "http://leaflet",
            EducationMaterials: null,
            IsOral: false,
            RoutesOfAdministration: [],
            PharmaceuticalForms: []);

        var reviewService = new Mock<ITempDrugReviewApplicationService>();
        reviewService
            .Setup(s => s.UpdateDrugFormLinksAsync(It.IsAny<UpdateDrugFormLinksFromTempDrugReviewTempDrugApplicationServiceDraft>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UpdateDrugFormLinksFromTempDrugReviewTempDrugApplicationServiceResult(
                TempDrugId: tempDrugId,
                NewStatus: TempDrugStatus.ReviewedAdded,
                LinkedDrugFormId: drugFormId,
                Note: "updated",
                DrugForm: drugForm));

        var handler = new UpdateDrugFormLinksFromTempDrugReviewHandler(reviewService.Object);

        ErrorOr<UpdatedDrugFormLinksFromTempDrugReviewResponse> result =
            await handler.Handle(SampleCommand(tempDrugId), CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.DrugForm.Leaflet.Should().Be("http://leaflet");
    }

    private static UpdateDrugFormLinksFromTempDrugReviewCommand SampleCommand(Guid tempDrugId) =>
        new(
            tempDrugId,
            new UpdateDrugFormLinksFromTempDrugReviewRequest(
                Note: null,
                DrugId: Guid.NewGuid(),
                DrugFormId: Guid.NewGuid(),
                CommercialName: "Apap",
                TypeOfProduct: "Ludzki",
                ActiveSubstanceIds: [Guid.NewGuid()],
                ProductIdentifier: "100",
                AtcCode: null,
                Dosage: null,
                Leaflet: "http://leaflet",
                EducationalMaterials: null,
                RouteOfAdministrationIds: null,
                PharmaceuticalFormIds: null));
}
