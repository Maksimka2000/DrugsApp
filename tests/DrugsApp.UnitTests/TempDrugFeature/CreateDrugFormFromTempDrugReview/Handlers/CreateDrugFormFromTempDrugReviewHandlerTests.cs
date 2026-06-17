using DrugsApp.Features.DrugFeature.Shared;
using DrugsApp.Features.TempDrugFeature.ApplicationServices;
using DrugsApp.Features.TempDrugFeature.CreateDrugFormFromTempDrugReview;
using DrugsApp.Domain.TempDrugFeature.Entities;
using ErrorOr;
using FluentAssertions;
using Moq;

namespace DrugsApp.UnitTests.TempDrugFeature.CreateDrugFormFromTempDrugReview.Handlers;

public sealed class CreateDrugFormFromTempDrugReviewHandlerTests
{
    [Fact]
    public async Task Handle_WhenApplicationServiceReturnsError_PropagatesError()
    {
        var reviewService = new Mock<ITempDrugReviewApplicationService>();
        reviewService
            .Setup(s => s.CreateDrugFormAsync(It.IsAny<CreateDrugFormFromTempDrugReviewTempDrugApplicationServiceDraft>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.NotFound(description: "The requested resource was not found"));

        var handler = new CreateDrugFormFromTempDrugReviewHandler(reviewService.Object);
        var command = SampleCommand(Guid.NewGuid());

        ErrorOr<CreatedDrugFormFromTempDrugReviewResponse> result =
            await handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
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
            Leaflet: null,
            EducationMaterials: null,
            IsOral: false,
            RoutesOfAdministration: [],
            PharmaceuticalForms: []);

        var reviewService = new Mock<ITempDrugReviewApplicationService>();
        reviewService
            .Setup(s => s.CreateDrugFormAsync(It.IsAny<CreateDrugFormFromTempDrugReviewTempDrugApplicationServiceDraft>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CreateDrugFormFromTempDrugReviewTempDrugApplicationServiceResult(
                TempDrugId: tempDrugId,
                NewStatus: TempDrugStatus.ReviewedAdded,
                LinkedDrugFormId: drugFormId,
                Note: null,
                DrugForm: drugForm));

        var handler = new CreateDrugFormFromTempDrugReviewHandler(reviewService.Object);

        ErrorOr<CreatedDrugFormFromTempDrugReviewResponse> result =
            await handler.Handle(SampleCommand(tempDrugId), CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.DrugForm.Id.Should().Be(drugFormId);
        result.Value.NewStatus.Should().Be(TempDrugStatus.ReviewedAdded);
    }

    private static CreateDrugFormFromTempDrugReviewCommand SampleCommand(Guid tempDrugId) =>
        new(
            tempDrugId,
            new CreateDrugFormFromTempDrugReviewRequest(
                Note: null,
                DrugId: Guid.NewGuid(),
                CommercialName: "Apap",
                TypeOfProduct: "Ludzki",
                ActiveSubstanceIds: [Guid.NewGuid()],
                ProductIdentifier: "100",
                AtcCode: null,
                Dosage: null,
                Leaflet: null,
                EducationalMaterials: null,
                RouteOfAdministrationIds: null,
                PharmaceuticalFormIds: null));
}
