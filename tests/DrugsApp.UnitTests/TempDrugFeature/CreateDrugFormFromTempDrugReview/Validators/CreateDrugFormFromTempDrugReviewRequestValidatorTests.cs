using DrugsApp.Features.TempDrugFeature.CreateDrugFormFromTempDrugReview;
using FluentAssertions;

namespace DrugsApp.UnitTests.TempDrugFeature.CreateDrugFormFromTempDrugReview.Validators;

public sealed class CreateDrugFormFromTempDrugReviewRequestValidatorTests
{
    private readonly CreateDrugFormFromTempDrugReviewRequestValidator _validator = new();

    [Fact]
    public async Task Validate_WhenDrugIdEmpty_ReturnsValidationError()
    {
        var request = new CreateDrugFormFromTempDrugReviewRequest(
            Note: null,
            DrugId: Guid.Empty,
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
            e.ErrorMessage == "DrugId must not be empty.");
    }

    [Fact]
    public async Task Validate_WhenDuplicateActiveSubstanceIds_ReturnsValidationError()
    {
        var substanceId = Guid.NewGuid();
        var request = new CreateDrugFormFromTempDrugReviewRequest(
            Note: null,
            DrugId: Guid.NewGuid(),
            CommercialName: "Apap",
            TypeOfProduct: "Ludzki",
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

    [Fact]
    public async Task Validate_WhenProductIdentifierNotPositiveInteger_ReturnsValidationError()
    {
        var request = new CreateDrugFormFromTempDrugReviewRequest(
            Note: null,
            DrugId: Guid.NewGuid(),
            CommercialName: "Apap",
            TypeOfProduct: "Ludzki",
            ActiveSubstanceIds: [Guid.NewGuid()],
            ProductIdentifier: "0",
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
    public async Task Validate_WhenEmptyRouteIdsProvided_ReturnsValidationError()
    {
        var request = new CreateDrugFormFromTempDrugReviewRequest(
            Note: null,
            DrugId: Guid.NewGuid(),
            CommercialName: "Apap",
            TypeOfProduct: "Ludzki",
            ActiveSubstanceIds: [Guid.NewGuid()],
            ProductIdentifier: "12345",
            AtcCode: null,
            Dosage: null,
            Leaflet: null,
            EducationalMaterials: null,
            RouteOfAdministrationIds: [],
            PharmaceuticalFormIds: null);

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.ErrorMessage == "RouteOfAdministrationIds must not be empty.");
    }
}
