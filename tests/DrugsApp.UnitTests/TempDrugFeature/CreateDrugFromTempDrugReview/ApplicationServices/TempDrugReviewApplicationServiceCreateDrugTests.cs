using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.TempDrugFeature.Entities;
using DrugsApp.Domain.TempDrugFeature.Repositories;
using DrugsApp.Features.DrugFeature.ApplicationServices;
using DrugsApp.Features.DrugFeature.Shared;
using DrugsApp.Features.TempDrugFeature.ApplicationServices;
using DrugsApp.Features.VetDrugFeature.ApplicationServices;
using ErrorOr;
using FluentAssertions;
using Moq;

namespace DrugsApp.UnitTests.TempDrugFeature.CreateDrugFromTempDrugReview.ApplicationServices;

public sealed class TempDrugReviewApplicationServiceCreateDrugTests
{
    [Fact]
    public async Task CreateDrugAsync_WhenTempDrugNotFound_ReturnsNotFound()
    {
        var tempDrugId = Guid.NewGuid();
        var tempDrugRepository = new Mock<ITempDrugRepository>();
        tempDrugRepository
            .Setup(r => r.GetByIdAsync(tempDrugId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TempDrug?)null);

        var service = BuildService(tempDrugRepository);

        ErrorOr<CreateDrugFromTempDrugReviewTempDrugApplicationServiceResult> result =
            await service.CreateDrugAsync(SampleDraft(tempDrugId), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        result.FirstError.Metadata!["resourceType"].Should().Be("tempDrug");
    }

    [Fact]
    public async Task CreateDrugAsync_WhenAlreadyInTerminalStatus_ReturnsConflict()
    {
        TempDrug tempDrug = CreateEligibleTempDrug();
        tempDrug.ChangeStatus(TempDrugStatus.ReviewedAdded);

        var tempDrugRepository = MockRepositoryReturning(tempDrug);
        var service = BuildService(tempDrugRepository);

        ErrorOr<CreateDrugFromTempDrugReviewTempDrugApplicationServiceResult> result =
            await service.CreateDrugAsync(SampleDraft(tempDrug.Id), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Conflict);
        result.FirstError.Description.Should().Be("TempDrug is already in a terminal status.");
    }

    [Fact]
    public async Task CreateDrugAsync_WhenReviewedResolved_ReturnsConflict()
    {
        TempDrug tempDrug = CreateEligibleTempDrug();
        tempDrug.ChangeStatus(TempDrugStatus.ReviewedResolved);

        var service = BuildService(MockRepositoryReturning(tempDrug));

        ErrorOr<CreateDrugFromTempDrugReviewTempDrugApplicationServiceResult> result =
            await service.CreateDrugAsync(SampleDraft(tempDrug.Id), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Conflict);
        result.FirstError.Description.Should().Be("TempDrug is already in a terminal status.");
    }

    [Fact]
    public async Task CreateDrugAsync_WhenAlreadyLinked_ReturnsConflict()
    {
        TempDrug tempDrug = CreateEligibleTempDrug();
        tempDrug.LinkToDrugForm(Guid.NewGuid());

        var service = BuildService(MockRepositoryReturning(tempDrug));

        ErrorOr<CreateDrugFromTempDrugReviewTempDrugApplicationServiceResult> result =
            await service.CreateDrugAsync(SampleDraft(tempDrug.Id), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Conflict);
        result.FirstError.Description.Should().Be("TempDrug is already linked to a canonical record.");
    }

    [Fact]
    public async Task CreateDrugAsync_WhenCanonicalizationFails_PropagatesError()
    {
        TempDrug tempDrug = CreateEligibleTempDrug();
        var canonicalization = new Mock<IDrugReviewCanonicalizationService>();
        canonicalization
            .Setup(s => s.CreateNewDrugFromReviewAsync(It.IsAny<CreateDrugFromReviewDrugApplicationServiceDraft>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Validation(description: "Some activeSubstanceIds were not found."));

        var service = BuildService(MockRepositoryReturning(tempDrug), canonicalization);

        ErrorOr<CreateDrugFromTempDrugReviewTempDrugApplicationServiceResult> result =
            await service.CreateDrugAsync(SampleDraft(tempDrug.Id), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Description.Should().Be("Some activeSubstanceIds were not found.");
    }

    [Fact]
    public async Task CreateDrugAsync_WhenEligible_LinksNoteAndSetsReviewedAdded()
    {
        TempDrug tempDrug = CreateEligibleTempDrug();
        var drugFormId = Guid.NewGuid();
        DrugDto drugDto = new(
            Id: Guid.NewGuid(),
            CommercialName: "Apap",
            TypeOfProduct: "Ludzki",
            TypeOfDrug: DrugType.Standard,
            DrugFormsCount: 1,
            ActiveSubstances: [],
            DrugForms:
            [
                new DrugFormDto(
                    Id: drugFormId,
                    ProductIdentifier: 12345,
                    AtcCode: null,
                    Dosage: null,
                    Leaflet: null,
                    EducationMaterials: null,
                    IsOral: false,
                    RoutesOfAdministration: [],
                    PharmaceuticalForms: [])
            ]);

        var canonicalization = new Mock<IDrugReviewCanonicalizationService>();
        canonicalization
            .Setup(s => s.CreateNewDrugFromReviewAsync(It.IsAny<CreateDrugFromReviewDrugApplicationServiceDraft>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(drugDto);

        var service = BuildService(MockRepositoryReturning(tempDrug), canonicalization);

        ErrorOr<CreateDrugFromTempDrugReviewTempDrugApplicationServiceResult> result =
            await service.CreateDrugAsync(SampleDraft(tempDrug.Id, note: "  reviewed  "), CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.TempDrugId.Should().Be(tempDrug.Id);
        result.Value.NewStatus.Should().Be(TempDrugStatus.ReviewedAdded);
        result.Value.LinkedDrugFormId.Should().Be(drugFormId);
        result.Value.Note.Should().Be("reviewed");
        tempDrug.Status.Should().Be(TempDrugStatus.ReviewedAdded);
        tempDrug.LinkedDrugFormId.Should().Be(drugFormId);
    }

    private static TempDrug CreateEligibleTempDrug() =>
        TempDrug.CreateImported(
            productIdentifier: "100",
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

    private static CreateDrugFromTempDrugReviewTempDrugApplicationServiceDraft SampleDraft(
        Guid tempDrugId,
        string? note = null) =>
        new(
            TempDrugId: tempDrugId,
            Note: note,
            CanonicalDrugApplicationServiceDraft: new CreateDrugFromReviewDrugApplicationServiceDraft(
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
