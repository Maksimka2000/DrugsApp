using DrugsApp.Features.TempDrugFeature.UpdateDrugFormLinksFromTempDrugReview;
using FluentAssertions;

namespace DrugsApp.UnitTests.TempDrugFeature.UpdateDrugFormLinksFromTempDrugReview.Validators;

public sealed class UpdateDrugFormLinksFromTempDrugReviewRequestValidatorTests
{
    private readonly UpdateDrugFormLinksFromTempDrugReviewRequestValidator _validator = new();

    [Fact]
    public async Task Validate_WhenNoLinksProvided_ReturnsValidationError()
    {
        var request = new UpdateDrugFormLinksFromTempDrugReviewRequest(
            Note: null,
            DrugId: Guid.NewGuid(),
            DrugFormId: Guid.NewGuid(),
            CommercialName: "Apap",
            TypeOfProduct: "Ludzki",
            ActiveSubstanceIds: [Guid.NewGuid()],
            ProductIdentifier: "12345",
            AtcCode: null,
            Dosage: null,
            Leaflet: null,
            EducationalMaterials: null,
            RouteOfAdministrationIds: null,
            PharmaceuticalFormIds: null);

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.ErrorMessage == "At least one of Leaflet or EducationalMaterials must be provided.");
    }

    [Fact]
    public async Task Validate_WhenDrugFormIdEmpty_ReturnsValidationError()
    {
        var request = new UpdateDrugFormLinksFromTempDrugReviewRequest(
            Note: null,
            DrugId: Guid.NewGuid(),
            DrugFormId: Guid.Empty,
            CommercialName: "Apap",
            TypeOfProduct: "Ludzki",
            ActiveSubstanceIds: [Guid.NewGuid()],
            ProductIdentifier: "12345",
            AtcCode: null,
            Dosage: null,
            Leaflet: "http://leaflet",
            EducationalMaterials: null,
            RouteOfAdministrationIds: null,
            PharmaceuticalFormIds: null);

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.ErrorMessage == "DrugFormId must not be empty.");
    }

    [Fact]
    public async Task Validate_WhenEducationalMaterialsProvided_IsValid()
    {
        var request = new UpdateDrugFormLinksFromTempDrugReviewRequest(
            Note: null,
            DrugId: Guid.NewGuid(),
            DrugFormId: Guid.NewGuid(),
            CommercialName: "Apap",
            TypeOfProduct: "Ludzki",
            ActiveSubstanceIds: [Guid.NewGuid()],
            ProductIdentifier: "12345",
            AtcCode: null,
            Dosage: null,
            Leaflet: null,
            EducationalMaterials: "http://edu",
            RouteOfAdministrationIds: null,
            PharmaceuticalFormIds: null);

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }
}
