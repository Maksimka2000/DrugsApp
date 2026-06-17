using DrugsApp.Domain.TempDrugFeature.Entities;
using DrugsApp.Domain.TempDrugFeature.Repositories;
using DrugsApp.Features.DrugFeature.ApplicationServices;
using DrugsApp.Features.DrugFeature.Shared;
using DrugsApp.Features.TempDrugFeature.ApplicationServices;
using DrugsApp.Features.VetDrugFeature.ApplicationServices;
using ErrorOr;
using FluentAssertions;
using Moq;

namespace DrugsApp.UnitTests.TempDrugFeature.CreateDrugFormFromTempDrugReview.ApplicationServices;

public sealed class TempDrugReviewApplicationServiceCreateDrugFormTests
{
    [Fact]
    public async Task CreateDrugFormAsync_WhenTempDrugNotFound_ReturnsNotFound()
    {
        var tempDrugId = Guid.NewGuid();
        var tempDrugRepository = new Mock<ITempDrugRepository>();
        tempDrugRepository
            .Setup(r => r.GetByIdAsync(tempDrugId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TempDrug?)null);

        var service = BuildService(tempDrugRepository);

        ErrorOr<CreateDrugFormFromTempDrugReviewTempDrugApplicationServiceResult> result =
            await service.CreateDrugFormAsync(SampleDraft(tempDrugId), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task CreateDrugFormAsync_WhenAlreadyLinked_ReturnsConflict()
    {
        TempDrug tempDrug = CreateEligibleTempDrug();
        tempDrug.LinkToDrugForm(Guid.NewGuid());

        var service = BuildService(MockRepositoryReturning(tempDrug));

        ErrorOr<CreateDrugFormFromTempDrugReviewTempDrugApplicationServiceResult> result =
            await service.CreateDrugFormAsync(SampleDraft(tempDrug.Id), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Conflict);
        result.FirstError.Description.Should().Be("TempDrug is already linked to a canonical record.");
    }

    [Fact]
    public async Task CreateDrugFormAsync_WhenLinkedToVetDrug_ReturnsConflict()
    {
        TempDrug tempDrug = CreateEligibleTempDrug();
        tempDrug.LinkToVetDrug(Guid.NewGuid());

        var service = BuildService(MockRepositoryReturning(tempDrug));

        ErrorOr<CreateDrugFormFromTempDrugReviewTempDrugApplicationServiceResult> result =
            await service.CreateDrugFormAsync(SampleDraft(tempDrug.Id), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Conflict);
        result.FirstError.Description.Should().Be("TempDrug is already linked to a canonical record.");
    }

    [Fact]
    public async Task CreateDrugFormAsync_WhenCanonicalizationFails_PropagatesError()
    {
        TempDrug tempDrug = CreateEligibleTempDrug();
        var canonicalization = new Mock<IDrugReviewCanonicalizationService>();
        canonicalization
            .Setup(s => s.CreateDrugFormForExistingDrugFromReviewAsync(It.IsAny<CreateDrugFormForExistingDrugFromReviewDrugApplicationServiceDraft>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.NotFound(description: "Drug not found."));

        var service = BuildService(MockRepositoryReturning(tempDrug), canonicalization);

        ErrorOr<CreateDrugFormFromTempDrugReviewTempDrugApplicationServiceResult> result =
            await service.CreateDrugFormAsync(SampleDraft(tempDrug.Id), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task CreateDrugFormAsync_WhenEligible_LinksAndSetsReviewedAdded()
    {
        TempDrug tempDrug = CreateEligibleTempDrug();
        var drugFormId = Guid.NewGuid();
        DrugFormDto drugFormDto = new(
            Id: drugFormId,
            ProductIdentifier: 54321,
            AtcCode: "N02BE01",
            Dosage: "500mg",
            Leaflet: null,
            EducationMaterials: null,
            IsOral: true,
            RoutesOfAdministration: [],
            PharmaceuticalForms: []);

        var canonicalization = new Mock<IDrugReviewCanonicalizationService>();
        canonicalization
            .Setup(s => s.CreateDrugFormForExistingDrugFromReviewAsync(It.IsAny<CreateDrugFormForExistingDrugFromReviewDrugApplicationServiceDraft>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(drugFormDto);

        var service = BuildService(MockRepositoryReturning(tempDrug), canonicalization);

        ErrorOr<CreateDrugFormFromTempDrugReviewTempDrugApplicationServiceResult> result =
            await service.CreateDrugFormAsync(SampleDraft(tempDrug.Id, note: "merged"), CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.NewStatus.Should().Be(TempDrugStatus.ReviewedAdded);
        result.Value.LinkedDrugFormId.Should().Be(drugFormId);
        result.Value.DrugForm.Id.Should().Be(drugFormId);
        tempDrug.LinkedDrugFormId.Should().Be(drugFormId);
    }

    private static TempDrug CreateEligibleTempDrug() =>
        TempDrug.CreateImported(
            productIdentifier: "200",
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

    private static CreateDrugFormFromTempDrugReviewTempDrugApplicationServiceDraft SampleDraft(
        Guid tempDrugId,
        string? note = null) =>
        new(
            TempDrugId: tempDrugId,
            Note: note,
            CanonicalDraft: new CreateDrugFormForExistingDrugFromReviewDrugApplicationServiceDraft(
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
