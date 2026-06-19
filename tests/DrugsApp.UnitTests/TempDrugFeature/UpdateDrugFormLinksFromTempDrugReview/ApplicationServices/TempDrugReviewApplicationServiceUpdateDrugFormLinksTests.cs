using DrugsApp.Domain.TempDrugFeature.Entities;
using DrugsApp.Domain.TempDrugFeature.Repositories;
using DrugsApp.Features.DrugFeature.ApplicationServices;
using DrugsApp.Features.DrugFeature.Shared;
using DrugsApp.Features.TempDrugFeature.ApplicationServices;
using DrugsApp.Features.VetDrugFeature.ApplicationServices;
using ErrorOr;
using FluentAssertions;
using Moq;

namespace DrugsApp.UnitTests.TempDrugFeature.UpdateDrugFormLinksFromTempDrugReview.ApplicationServices;

public sealed class TempDrugReviewApplicationServiceUpdateDrugFormLinksTests
{
    [Fact]
    public async Task UpdateDrugFormLinksAsync_WhenTempDrugNotFound_ReturnsNotFound()
    {
        var tempDrugId = Guid.NewGuid();
        var tempDrugRepository = new Mock<ITempDrugRepository>();
        tempDrugRepository
            .Setup(r => r.GetByIdAsync(tempDrugId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TempDrug?)null);

        var service = BuildService(tempDrugRepository);

        ErrorOr<UpdateDrugFormLinksFromTempDrugReviewTempDrugApplicationServiceResult> result =
            await service.UpdateDrugFormLinksAsync(SampleDraft(tempDrugId), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task UpdateDrugFormLinksAsync_WhenTerminalStatus_ReturnsConflict()
    {
        TempDrug tempDrug = CreateEligibleTempDrug();
        tempDrug.ChangeStatus(TempDrugStatus.ReviewedResolved);

        var service = BuildService(MockRepositoryReturning(tempDrug));

        ErrorOr<UpdateDrugFormLinksFromTempDrugReviewTempDrugApplicationServiceResult> result =
            await service.UpdateDrugFormLinksAsync(SampleDraft(tempDrug.Id), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task UpdateDrugFormLinksAsync_WhenAlreadyLinked_ReturnsConflict()
    {
        TempDrug tempDrug = CreateEligibleTempDrug();
        tempDrug.LinkToDrugForm(Guid.NewGuid());

        var service = BuildService(MockRepositoryReturning(tempDrug));

        ErrorOr<UpdateDrugFormLinksFromTempDrugReviewTempDrugApplicationServiceResult> result =
            await service.UpdateDrugFormLinksAsync(SampleDraft(tempDrug.Id), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Conflict);
        result.FirstError.Description.Should().Be("TempDrug is already linked to a canonical record.");
    }

    [Fact]
    public async Task UpdateDrugFormLinksAsync_WhenCanonicalizationFails_PropagatesError()
    {
        TempDrug tempDrug = CreateEligibleTempDrug();
        var canonicalization = new Mock<IDrugReviewCanonicalizationService>();
        canonicalization
            .Setup(s => s.UpdateDrugFormLinksFromReviewAsync(It.IsAny<UpdateDrugFormLinksFromReviewDrugApplicationServiceDraft>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Validation(description: "The provided drugFormId does not belong to the provided drugId."));

        var service = BuildService(MockRepositoryReturning(tempDrug), canonicalization);

        ErrorOr<UpdateDrugFormLinksFromTempDrugReviewTempDrugApplicationServiceResult> result =
            await service.UpdateDrugFormLinksAsync(SampleDraft(tempDrug.Id), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task UpdateDrugFormLinksAsync_WhenEligible_UpdatesLinksAndStatus()
    {
        TempDrug tempDrug = CreateEligibleTempDrug();
        var drugFormId = Guid.NewGuid();
        DrugFormDto drugFormDto = new(
            Id: drugFormId,
            ProductIdentifier: 100,
            AtcCode: null,
            Dosage: null,
            Leaflet: "http://new-leaflet",
            EducationMaterials: null,
            IsOral: false,
            RoutesOfAdministration: [],
            PharmaceuticalForms: []);

        var canonicalization = new Mock<IDrugReviewCanonicalizationService>();
        canonicalization
            .Setup(s => s.UpdateDrugFormLinksFromReviewAsync(It.IsAny<UpdateDrugFormLinksFromReviewDrugApplicationServiceDraft>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(drugFormDto);

        var service = BuildService(MockRepositoryReturning(tempDrug), canonicalization);

        ErrorOr<UpdateDrugFormLinksFromTempDrugReviewTempDrugApplicationServiceResult> result =
            await service.UpdateDrugFormLinksAsync(SampleDraft(tempDrug.Id, note: "updated links"), CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.NewStatus.Should().Be(TempDrugStatus.ReviewedAdded);
        result.Value.LinkedDrugFormId.Should().Be(drugFormId);
        result.Value.DrugForm.Leaflet.Should().Be("http://new-leaflet");
        tempDrug.Status.Should().Be(TempDrugStatus.ReviewedAdded);
    }

    private static TempDrug CreateEligibleTempDrug() =>
        TempDrug.CreateImported(
            productIdentifier: "300",
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

    private static UpdateDrugFormLinksFromTempDrugReviewTempDrugApplicationServiceDraft SampleDraft(
        Guid tempDrugId,
        string? note = null) =>
        new(
            TempDrugId: tempDrugId,
            Note: note,
            CanonicalDrugApplicationServiceDraft: new UpdateDrugFormLinksFromReviewDrugApplicationServiceDraft(
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

    private static Mock<ITempDrugRepository> MockRepositoryReturning(TempDrug tempDrug)
    {
        var repository = new Mock<ITempDrugRepository>();
        repository
            .Setup(r => r.GetByIdAsync(tempDrug.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tempDrug);
        return repository;
    }

    private static TempDrugReviewApplicationService BuildService(
        Mock<ITempDrugRepository> tempDrugRepository,
        Mock<IDrugReviewCanonicalizationService>? canonicalization = null) =>
        new(
            tempDrugRepository.Object,
            (canonicalization ?? new Mock<IDrugReviewCanonicalizationService>()).Object,
            new Mock<IVetDrugCanonicalizationService>().Object);
}
