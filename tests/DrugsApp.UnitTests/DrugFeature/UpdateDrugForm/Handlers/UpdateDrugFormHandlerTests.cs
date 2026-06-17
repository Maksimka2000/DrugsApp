using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.DrugFeature.Repositories;
using DrugsApp.Domain.PharmaceuticalFormFeature.Entities;
using DrugsApp.Domain.PharmaceuticalFormFeature.Repositories;
using DrugsApp.Domain.RouteOfAdministrationFeature.Entities;
using DrugsApp.Domain.RouteOfAdministrationFeature.Repositories;
using DrugsApp.Features.DrugFeature.UpdateDrugForm;
using ErrorOr;
using FluentAssertions;
using Moq;

namespace DrugsApp.UnitTests.DrugFeature.UpdateDrugForm.Handlers;

public sealed class UpdateDrugFormHandlerTests
{
    [Fact]
    public async Task Handle_WhenDrugFormNotFound_ReturnsNotFound()
    {
        var drugFormId = Guid.NewGuid();
        var drugFormRepository = new Mock<IDrugFormRepository>();
        drugFormRepository
            .Setup(r => r.GetByIdAsync(drugFormId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DrugForm?)null);

        var handler = BuildHandler(drugFormRepository);

        ErrorOr<UpdatedDrugFormResponse> result = await handler.Handle(
            new UpdateDrugFormCommand(
                Guid.NewGuid(),
                drugFormId,
                new UpdateDrugFormRequest(
                    AtcCode: null,
                    Dosage: null,
                    Leaflet: null,
                    EducationMaterials: null,
                    RouteOfAdministrationIds: null,
                    PharmaceuticalFormIds: null,
                    ClearAtcCode: null,
                    ClearDosage: null,
                    ClearLeaflet: null,
                    ClearEducationMaterials: null)),
            CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        result.FirstError.Metadata!["resourceType"].Should().Be("DrugForm");
    }

    [Fact]
    public async Task Handle_WhenDrugFormBelongsToDifferentDrug_ReturnsNotFound()
    {
        DrugForm drugForm = DrugForm.Create(
            Guid.NewGuid(),
            productIdentifier: 100,
            atcCode: "N02BE01",
            dosage: "500mg",
            leaflet: null,
            educationMaterials: null);

        var drugFormRepository = new Mock<IDrugFormRepository>();
        drugFormRepository
            .Setup(r => r.GetByIdAsync(drugForm.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(drugForm);

        var handler = BuildHandler(drugFormRepository);

        ErrorOr<UpdatedDrugFormResponse> result = await handler.Handle(
            new UpdateDrugFormCommand(
                Guid.NewGuid(),
                drugForm.Id,
                new UpdateDrugFormRequest(
                    AtcCode: null,
                    Dosage: null,
                    Leaflet: null,
                    EducationMaterials: null,
                    RouteOfAdministrationIds: null,
                    PharmaceuticalFormIds: null,
                    ClearAtcCode: null,
                    ClearDosage: null,
                    ClearLeaflet: null,
                    ClearEducationMaterials: null)),
            CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_WhenRouteIdNotFound_ReturnsValidationError()
    {
        var drugId = Guid.NewGuid();
        DrugForm drugForm = DrugForm.Create(
            drugId,
            productIdentifier: 100,
            atcCode: "N02BE01",
            dosage: "500mg",
            leaflet: null,
            educationMaterials: null);
        RouteOfAdministration existingRoute = RouteOfAdministration.Create("Doustna");
        drugForm.AddRouteOfAdministration(existingRoute);

        var missingRouteId = Guid.NewGuid();
        var drugFormRepository = new Mock<IDrugFormRepository>();
        drugFormRepository
            .Setup(r => r.GetByIdAsync(drugForm.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(drugForm);

        var routeRepository = new Mock<IRouteOfAdministrationRepository>();
        routeRepository
            .Setup(r => r.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = BuildHandler(drugFormRepository, routeRepository);

        ErrorOr<UpdatedDrugFormResponse> result = await handler.Handle(
            new UpdateDrugFormCommand(
                drugId,
                drugForm.Id,
                new UpdateDrugFormRequest(
                    AtcCode: null,
                    Dosage: null,
                    Leaflet: null,
                    EducationMaterials: null,
                    RouteOfAdministrationIds: [existingRoute.Id, missingRouteId],
                    PharmaceuticalFormIds: null,
                    ClearAtcCode: null,
                    ClearDosage: null,
                    ClearLeaflet: null,
                    ClearEducationMaterials: null)),
            CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Description.Should().Be("One or more routes of administration were not found.");
    }

    [Fact]
    public async Task Handle_WhenUpdatingDosage_ReturnsUpdatedResponse()
    {
        var drugId = Guid.NewGuid();
        DrugForm drugForm = DrugForm.Create(
            drugId,
            productIdentifier: 100,
            atcCode: "N02BE01",
            dosage: "500mg",
            leaflet: null,
            educationMaterials: null);

        var drugFormRepository = new Mock<IDrugFormRepository>();
        drugFormRepository
            .Setup(r => r.GetByIdAsync(drugForm.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(drugForm);

        var handler = BuildHandler(drugFormRepository);

        ErrorOr<UpdatedDrugFormResponse> result = await handler.Handle(
            new UpdateDrugFormCommand(
                drugId,
                drugForm.Id,
                new UpdateDrugFormRequest(
                    AtcCode: null,
                    Dosage: "1000mg",
                    Leaflet: null,
                    EducationMaterials: null,
                    RouteOfAdministrationIds: null,
                    PharmaceuticalFormIds: null,
                    ClearAtcCode: null,
                    ClearDosage: null,
                    ClearLeaflet: null,
                    ClearEducationMaterials: null)),
            CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Dosage.Should().Be("1000mg");
    }

    private static UpdateDrugFormHandler BuildHandler(
        Mock<IDrugFormRepository>? drugFormRepository = null,
        Mock<IRouteOfAdministrationRepository>? routeRepository = null,
        Mock<IPharmaceuticalFormRepository>? pharmaceuticalFormRepository = null)
    {
        return new UpdateDrugFormHandler(
            (drugFormRepository ?? new Mock<IDrugFormRepository>()).Object,
            (routeRepository ?? new Mock<IRouteOfAdministrationRepository>()).Object,
            (pharmaceuticalFormRepository ?? new Mock<IPharmaceuticalFormRepository>()).Object);
    }
}
