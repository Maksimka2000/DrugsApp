using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Features.TempDrugFeature.CreateDrugFromTempDrugReview;
using FluentAssertions;

namespace DrugsApp.UnitTests.TempDrugFeature.CreateDrugFromTempDrugReview.Validators;

public sealed class CreateDrugFromTempDrugReviewRequestValidatorTests
{
    private readonly CreateDrugFromTempDrugReviewRequestValidator _validator = new();

    [Fact]
    public async Task Validate_WhenActiveSubstanceIdsEmpty_ReturnsValidationError()
    {
        var request = new CreateDrugFromTempDrugReviewRequest(
            Note: null,
            CommercialName: "Apap",
            TypeOfProduct: "Ludzki",
            TypeOfDrug: DrugType.Standard,
            ActiveSubstanceIds: [],
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
            e.ErrorMessage == "ActiveSubstanceIds must not be empty.");
    }

    [Fact]
    public async Task Validate_WhenProductIdentifierNotPositiveInteger_ReturnsValidationError()
    {
        var request = new CreateDrugFromTempDrugReviewRequest(
            Note: null,
            CommercialName: "Apap",
            TypeOfProduct: "Ludzki",
            TypeOfDrug: DrugType.Standard,
            ActiveSubstanceIds: [Guid.NewGuid()],
            ProductIdentifier: "abc",
            AtcCode: null,
            Dosage: null,
            Leaflet: null,
            EducationalMaterials: null,
            RouteOfAdministrationIds: null,
            PharmaceuticalFormIds: null);

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.ErrorMessage == "ProductIdentifier must be a positive integer.");
    }

    [Fact]
    public async Task Validate_WhenCommercialNameEmpty_ReturnsValidationError()
    {
        var request = new CreateDrugFromTempDrugReviewRequest(
            Note: null,
            CommercialName: "",
            TypeOfProduct: "Ludzki",
            TypeOfDrug: DrugType.Standard,
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
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateDrugFromTempDrugReviewRequest.CommercialName));
    }

    [Fact]
    public async Task Validate_WhenDuplicateActiveSubstanceIds_ReturnsValidationError()
    {
        var substanceId = Guid.NewGuid();
        var request = new CreateDrugFromTempDrugReviewRequest(
            Note: null,
            CommercialName: "Apap",
            TypeOfProduct: "Ludzki",
            TypeOfDrug: DrugType.Standard,
            ActiveSubstanceIds: [substanceId, substanceId],
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
            e.ErrorMessage == "Duplicate activeSubstanceIds are not allowed.");
    }
}
