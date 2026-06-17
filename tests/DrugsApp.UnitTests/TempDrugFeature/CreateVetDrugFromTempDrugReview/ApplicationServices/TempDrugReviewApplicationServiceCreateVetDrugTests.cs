using DrugsApp.Domain.TempDrugFeature.Entities;
using DrugsApp.Domain.TempDrugFeature.Repositories;
using DrugsApp.Features.DrugFeature.ApplicationServices;
using DrugsApp.Features.TempDrugFeature.ApplicationServices;
using DrugsApp.Features.VetDrugFeature.ApplicationServices;
using DrugsApp.Features.VetDrugFeature.Shared;
using ErrorOr;
using FluentAssertions;
using Moq;

namespace DrugsApp.UnitTests.TempDrugFeature.CreateVetDrugFromTempDrugReview.ApplicationServices;

public sealed class TempDrugReviewApplicationServiceCreateVetDrugTests
{
    [Fact]
    public async Task CreateVetDrugAsync_WhenTempDrugNotFound_ReturnsNotFound()
    {
        var tempDrugId = Guid.NewGuid();
        var tempDrugRepository = new Mock<ITempDrugRepository>();
        tempDrugRepository
            .Setup(r => r.GetByIdAsync(tempDrugId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TempDrug?)null);

        var service = BuildService(tempDrugRepository);

        ErrorOr<CreateVetDrugFromTempDrugReviewResult> result =
            await service.CreateVetDrugAsync(SampleDraft(tempDrugId), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task CreateVetDrugAsync_WhenTerminalStatus_ReturnsConflict()
    {
        TempDrug tempDrug = CreateEligibleTempDrug();
        tempDrug.ChangeStatus(TempDrugStatus.ReviewedAdded);

        var service = BuildService(MockRepositoryReturning(tempDrug));

        ErrorOr<CreateVetDrugFromTempDrugReviewResult> result =
            await service.CreateVetDrugAsync(SampleDraft(tempDrug.Id), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task CreateVetDrugAsync_WhenAlreadyLinkedToDrugForm_ReturnsConflict()
    {
        TempDrug tempDrug = CreateEligibleTempDrug();
        tempDrug.LinkToDrugForm(Guid.NewGuid());

        var service = BuildService(MockRepositoryReturning(tempDrug));

        ErrorOr<CreateVetDrugFromTempDrugReviewResult> result =
            await service.CreateVetDrugAsync(SampleDraft(tempDrug.Id), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Conflict);
        result.FirstError.Description.Should().Be("TempDrug is already linked to a canonical record.");
    }

    [Fact]
    public async Task CreateVetDrugAsync_WhenCanonicalizationFails_PropagatesError()
    {
        TempDrug tempDrug = CreateEligibleTempDrug();
        var vetCanonicalization = new Mock<IVetDrugCanonicalizationService>();
        vetCanonicalization
            .Setup(s => s.CreateVetDrugFromReviewAsync(It.IsAny<CreateVetDrugFromReviewVetDrugApplicationServiceDraft>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Conflict(description: "A matching veterinary drug already exists."));

        var service = BuildService(MockRepositoryReturning(tempDrug), vetCanonicalization);

        ErrorOr<CreateVetDrugFromTempDrugReviewResult> result =
            await service.CreateVetDrugAsync(SampleDraft(tempDrug.Id), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task CreateVetDrugAsync_WhenEligible_LinksVetDrugAndSetsReviewedAdded()
    {
        TempDrug tempDrug = CreateEligibleTempDrug();
        var vetDrugId = Guid.NewGuid();
        VetDrugDto vetDrugDto = new(
            Id: vetDrugId,
            ProductIdentifier: 777,
            CommercialName: "VetApap",
            TypeOfProduct: "Weterynaryjny");

        var vetCanonicalization = new Mock<IVetDrugCanonicalizationService>();
        vetCanonicalization
            .Setup(s => s.CreateVetDrugFromReviewAsync(It.IsAny<CreateVetDrugFromReviewVetDrugApplicationServiceDraft>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vetDrugDto);

        var service = BuildService(MockRepositoryReturning(tempDrug), vetCanonicalization);

        ErrorOr<CreateVetDrugFromTempDrugReviewResult> result =
            await service.CreateVetDrugAsync(SampleDraft(tempDrug.Id, note: "vet review"), CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.NewStatus.Should().Be(TempDrugStatus.ReviewedAdded);
        result.Value.LinkedVetDrugId.Should().Be(vetDrugId);
        result.Value.VetDrug.CommercialName.Should().Be("VetApap");
        tempDrug.LinkedDrugVetId.Should().Be(vetDrugId);
        tempDrug.LinkedDrugFormId.Should().BeNull();
    }

    private static TempDrug CreateEligibleTempDrug() =>
        TempDrug.CreateImported(
            productIdentifier: "400",
            commercialName: "VetApap",
            activeSubstance: "Paracetamol",
            typeOfProduct: "Weterynaryjny",
            routeOfAdministration: null,
            pharmaceuticalFormulation: null,
            atcCode: null,
            leaflet: null,
            educationalMaterials: null,
            dosage: null,
            linkedImportId: Guid.NewGuid()).Value;

    private static CreateVetDrugFromTempDrugReviewDraft SampleDraft(
        Guid tempDrugId,
        string? note = null) =>
        new(
            TempDrugId: tempDrugId,
            Note: note,
            CanonicalDraft: new CreateVetDrugFromReviewVetDrugApplicationServiceDraft(
                CommercialName: "VetApap",
                TypeOfProduct: "Weterynaryjny",
                ProductIdentifier: "777"));

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
        Mock<IVetDrugCanonicalizationService>? vetCanonicalization = null) =>
        new(
            tempDrugRepository.Object,
            new Mock<IDrugReviewCanonicalizationService>().Object,
            (vetCanonicalization ?? new Mock<IVetDrugCanonicalizationService>()).Object);
}
