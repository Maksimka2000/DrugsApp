using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.ActiveSubstanceFeature.Repositories;
using DrugsApp.Domain.DrugFeature.DomainServices;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.DrugFeature.Repositories;
using DrugsApp.Domain.PharmaceuticalFormFeature.Entities;
using DrugsApp.Domain.RouteOfAdministrationFeature.Entities;
using DrugsApp.Domain.PharmaceuticalFormFeature.Repositories;
using DrugsApp.Domain.RouteOfAdministrationFeature.Repositories;
using DrugsApp.Features.DrugFeature.ApplicationServices;
using DrugsApp.Features.DrugFeature.Shared;
using ErrorOr;
using FluentAssertions;
using Moq;

namespace DrugsApp.UnitTests.DrugFeature.ReviewDrugCanonicalization.ApplicationServices;

public sealed class DrugReviewCanonicalizationServiceTests
{
    [Fact]
    public async Task CreateNewDrugFromReviewAsync_WhenProductIdentifierInvalid_ReturnsValidationError()
    {
        var service = BuildService();

        ErrorOr<DrugDto> result = await service.CreateNewDrugFromReviewAsync(
            new CreateDrugFromReviewDrugApplicationServiceDraft(
                CommercialName: "Apap",
                TypeOfProduct: "Ludzki",
                TypeOfDrug: DrugType.Standard,
                ActiveSubstanceIds: [Guid.NewGuid()],
                ProductIdentifier: "0",
                AtcCode: null,
                Dosage: null,
                Leaflet: null,
                EducationalMaterials: null,
                RouteOfAdministrationIds: null,
                PharmaceuticalFormIds: null),
            CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Description.Should().Be("ProductIdentifier must be a positive integer.");
    }

    [Fact]
    public async Task CreateNewDrugFromReviewAsync_WhenActiveSubstanceMissing_ReturnsValidationError()
    {
        var missingId = Guid.NewGuid();
        var activeSubstanceRepository = new Mock<IActiveSubstanceRepository>();
        activeSubstanceRepository
            .Setup(r => r.GetByIdsForLinkingAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var service = BuildService(activeSubstanceRepository: activeSubstanceRepository);

        ErrorOr<DrugDto> result = await service.CreateNewDrugFromReviewAsync(
            new CreateDrugFromReviewDrugApplicationServiceDraft(
                CommercialName: "Apap",
                TypeOfProduct: "Ludzki",
                TypeOfDrug: DrugType.Standard,
                ActiveSubstanceIds: [missingId],
                ProductIdentifier: "12345",
                AtcCode: null,
                Dosage: null,
                Leaflet: null,
                EducationalMaterials: null,
                RouteOfAdministrationIds: null,
                PharmaceuticalFormIds: null),
            CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Description.Should().Be("Some activeSubstanceIds were not found.");
    }

    [Fact]
    public async Task CreateNewDrugFromReviewAsync_WhenDuplicationCandidateExists_ReturnsConflict()
    {
        ActiveSubstance substance = ActiveSubstance.Create("Paracetamol");
        RouteOfAdministration route = RouteOfAdministration.Create("Doustna");
        PharmaceuticalForm pharmaceuticalForm = PharmaceuticalForm.Create("Tabletka");

        Drug existingDrug = Drug.Create("Apap", "Ludzki", DrugType.Standard, [substance], drugForms: null);
        DrugForm existingForm = DrugForm.Create(
            existingDrug.Id,
            productIdentifier: 12345,
            atcCode: "N02BE01",
            dosage: "500mg",
            leaflet: "http://leaflet",
            educationMaterials: "http://edu");
        existingForm.AddRouteOfAdministration(route);
        existingForm.AddPharmaceuticalForm(pharmaceuticalForm);
        existingDrug.AddDrugForm(existingForm);

        var drugRepository = new Mock<IDrugRepository>();
        drugRepository
            .Setup(r => r.SearchSimilarCandidatesAsync("Apap", "Ludzki", It.IsAny<CancellationToken>()))
            .ReturnsAsync([existingDrug]);

        var activeSubstanceRepository = new Mock<IActiveSubstanceRepository>();
        activeSubstanceRepository
            .Setup(r => r.GetByIdsForLinkingAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([substance]);

        var routeRepository = new Mock<IRouteOfAdministrationRepository>();
        routeRepository
            .Setup(r => r.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([route]);

        var pharmaceuticalFormRepository = new Mock<IPharmaceuticalFormRepository>();
        pharmaceuticalFormRepository
            .Setup(r => r.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([pharmaceuticalForm]);

        var service = BuildService(
            drugRepository: drugRepository,
            activeSubstanceRepository: activeSubstanceRepository,
            routeRepository: routeRepository,
            pharmaceuticalFormRepository: pharmaceuticalFormRepository);

        ErrorOr<DrugDto> result = await service.CreateNewDrugFromReviewAsync(
            new CreateDrugFromReviewDrugApplicationServiceDraft(
                CommercialName: "Apap",
                TypeOfProduct: "Ludzki",
                TypeOfDrug: DrugType.Standard,
                ActiveSubstanceIds: [substance.Id],
                ProductIdentifier: "99999",
                AtcCode: "N02BE01",
                Dosage: "500mg",
                Leaflet: "http://leaflet",
                EducationalMaterials: "http://edu",
                RouteOfAdministrationIds: [route.Id],
                PharmaceuticalFormIds: [pharmaceuticalForm.Id]),
            CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Conflict);
        result.FirstError.Description.Should().Be("A matching drug form already exists.");
    }

    [Fact]
    public async Task CreateDrugFormForExistingDrugFromReviewAsync_WhenDrugNotFound_ReturnsNotFound()
    {
        var drugId = Guid.NewGuid();
        var drugRepository = new Mock<IDrugRepository>();
        drugRepository
            .Setup(r => r.GetByIdAsync(drugId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Drug?)null);

        var service = BuildService(drugRepository: drugRepository);

        ErrorOr<DrugFormDto> result = await service.CreateDrugFormForExistingDrugFromReviewAsync(
            new CreateDrugFormForExistingDrugFromReviewDrugApplicationServiceDraft(
                DrugId: drugId,
                CommercialName: "Apap",
                TypeOfProduct: "Ludzki",
                ActiveSubstanceIds: [Guid.NewGuid()],
                ProductIdentifier: "12345",
                AtcCode: null,
                Dosage: null,
                Leaflet: null,
                EducationalMaterials: null,
                RouteOfAdministrationIds: null,
                PharmaceuticalFormIds: null),
            CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task UpdateDrugFormLinksFromReviewAsync_WhenDrugFormBelongsToDifferentDrug_ReturnsValidationError()
    {
        var drugId = Guid.NewGuid();
        var otherDrugId = Guid.NewGuid();
        var drugFormId = Guid.NewGuid();
        ActiveSubstance substance = ActiveSubstance.Create("Paracetamol");
        Drug drug = Drug.Create("Apap", "Ludzki", DrugType.Standard, [substance], drugForms: null);
        DrugForm drugForm = DrugForm.Create(otherDrugId, productIdentifier: 100);

        var drugRepository = new Mock<IDrugRepository>();
        drugRepository.Setup(r => r.GetByIdAsync(drugId, It.IsAny<CancellationToken>())).ReturnsAsync(drug);

        var drugFormRepository = new Mock<IDrugFormRepository>();
        drugFormRepository.Setup(r => r.GetByIdAsync(drugFormId, It.IsAny<CancellationToken>())).ReturnsAsync(drugForm);

        var service = BuildService(drugRepository: drugRepository, drugFormRepository: drugFormRepository);

        ErrorOr<DrugFormDto> result = await service.UpdateDrugFormLinksFromReviewAsync(
            new UpdateDrugFormLinksFromReviewDrugApplicationServiceDraft(
                DrugId: drugId,
                DrugFormId: drugFormId,
                CommercialName: "Apap",
                TypeOfProduct: "Ludzki",
                ActiveSubstanceIds: [substance.Id],
                ProductIdentifier: "100",
                AtcCode: null,
                Dosage: null,
                Leaflet: "http://new",
                EducationalMaterials: null,
                RouteOfAdministrationIds: null,
                PharmaceuticalFormIds: null),
            CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Description.Should().Be("The provided drugFormId does not belong to the provided drugId.");
    }

    private static DrugReviewCanonicalizationService BuildService(
        Mock<IDrugRepository>? drugRepository = null,
        Mock<IActiveSubstanceRepository>? activeSubstanceRepository = null,
        Mock<IRouteOfAdministrationRepository>? routeRepository = null,
        Mock<IPharmaceuticalFormRepository>? pharmaceuticalFormRepository = null,
        Mock<IDrugFormRepository>? drugFormRepository = null)
    {
        return new DrugReviewCanonicalizationService(
            (drugRepository ?? new Mock<IDrugRepository>()).Object,
            (activeSubstanceRepository ?? new Mock<IActiveSubstanceRepository>()).Object,
            (routeRepository ?? new Mock<IRouteOfAdministrationRepository>()).Object,
            (pharmaceuticalFormRepository ?? new Mock<IPharmaceuticalFormRepository>()).Object,
            new DrugSimilarityDomainService(),
            (drugFormRepository ?? new Mock<IDrugFormRepository>()).Object);
    }
}
