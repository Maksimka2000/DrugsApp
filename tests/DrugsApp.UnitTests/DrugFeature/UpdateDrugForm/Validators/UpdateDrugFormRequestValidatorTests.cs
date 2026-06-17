using DrugsApp.Features.DrugFeature.UpdateDrugForm;
using FluentAssertions;

namespace DrugsApp.UnitTests.DrugFeature.UpdateDrugForm.Validators;

public sealed class UpdateDrugFormRequestValidatorTests
{
    private readonly UpdateDrugFormRequestValidator _validator = new();

    [Fact]
    public async Task Validate_WhenRouteIdsEmpty_ReturnsValidationError()
    {
        var request = new UpdateDrugFormRequest(
            AtcCode: null,
            ClearAtcCode: null,
            Dosage: null,
            ClearDosage: null,
            Leaflet: null,
            ClearLeaflet: null,
            EducationMaterials: null,
            ClearEducationMaterials: null,
            RouteOfAdministrationIds: [],
            PharmaceuticalFormIds: null);

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.ErrorMessage == "Drug form must have at least one route of administration.");
    }

    [Fact]
    public async Task Validate_WhenDuplicateRouteIds_ReturnsValidationError()
    {
        var routeId = Guid.NewGuid();
        var request = new UpdateDrugFormRequest(
            AtcCode: null,
            ClearAtcCode: null,
            Dosage: null,
            ClearDosage: null,
            Leaflet: null,
            ClearLeaflet: null,
            EducationMaterials: null,
            ClearEducationMaterials: null,
            RouteOfAdministrationIds: [routeId, routeId],
            PharmaceuticalFormIds: null);

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.ErrorMessage == "Duplicate RouteOfAdministrationIds values are not allowed.");
    }
}
