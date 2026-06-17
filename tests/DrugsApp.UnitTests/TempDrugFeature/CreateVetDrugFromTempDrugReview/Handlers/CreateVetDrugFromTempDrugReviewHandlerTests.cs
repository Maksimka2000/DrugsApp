using DrugsApp.Features.TempDrugFeature.ApplicationServices;
using DrugsApp.Features.TempDrugFeature.CreateVetDrugFromTempDrugReview;
using DrugsApp.Features.VetDrugFeature.Shared;
using DrugsApp.Domain.TempDrugFeature.Entities;
using ErrorOr;
using FluentAssertions;
using Moq;

namespace DrugsApp.UnitTests.TempDrugFeature.CreateVetDrugFromTempDrugReview.Handlers;

public sealed class CreateVetDrugFromTempDrugReviewHandlerTests
{
    [Fact]
    public async Task Handle_WhenApplicationServiceReturnsError_PropagatesError()
    {
        var reviewService = new Mock<ITempDrugReviewApplicationService>();
        reviewService
            .Setup(s => s.CreateVetDrugAsync(It.IsAny<CreateVetDrugFromTempDrugReviewDraft>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Conflict(description: "TempDrug is already in a terminal status."));

        var handler = new CreateVetDrugFromTempDrugReviewHandler(reviewService.Object);

        ErrorOr<CreatedVetDrugFromTempDrugReviewResponse> result =
            await handler.Handle(SampleCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task Handle_WhenApplicationServiceSucceeds_ReturnsMappedResponse()
    {
        var tempDrugId = Guid.NewGuid();
        var vetDrugId = Guid.NewGuid();
        VetDrugDto vetDrug = new(
            Id: vetDrugId,
            ProductIdentifier: 500,
            CommercialName: "VetApap",
            TypeOfProduct: "Weterynaryjny");

        var reviewService = new Mock<ITempDrugReviewApplicationService>();
        reviewService
            .Setup(s => s.CreateVetDrugAsync(It.IsAny<CreateVetDrugFromTempDrugReviewDraft>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CreateVetDrugFromTempDrugReviewResult(
                TempDrugId: tempDrugId,
                NewStatus: TempDrugStatus.ReviewedAdded,
                LinkedVetDrugId: vetDrugId,
                Note: null,
                VetDrug: vetDrug));

        var handler = new CreateVetDrugFromTempDrugReviewHandler(reviewService.Object);

        ErrorOr<CreatedVetDrugFromTempDrugReviewResponse> result =
            await handler.Handle(SampleCommand(tempDrugId), CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.LinkedVetDrugId.Should().Be(vetDrugId);
        result.Value.VetDrug.CommercialName.Should().Be("VetApap");
    }

    private static CreateVetDrugFromTempDrugReviewCommand SampleCommand(Guid tempDrugId) =>
        new(
            tempDrugId,
            new CreateVetDrugFromTempDrugReviewRequest(
                Note: null,
                CommercialName: "VetApap",
                TypeOfProduct: "Weterynaryjny",
                ProductIdentifier: "500"));
}
