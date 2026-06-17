using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.TempDrugFeature.Entities;
using DrugsApp.Features.TempDrugFeature.CreateDrugFromTempDrugReview;
using DrugsApp.Features.TempDrugFeature.CreateDrugFormFromTempDrugReview;
using DrugsApp.Features.TempDrugFeature.UpdateDrugFormLinksFromTempDrugReview;
using ErrorOr;
using FluentAssertions;
using FluentValidation.Results;

namespace DrugsApp.UnitTests.TempDrugFeature;

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

        ValidationResult? result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.ErrorMessage == "ActiveSubstanceIds must not be empty.");
    }
}

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

        ValidationResult? result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.ErrorMessage == "DrugId must not be empty.");
    }
}

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

        ValidationResult? result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.ErrorMessage == "At least one of Leaflet or EducationalMaterials must be provided.");
    }
}

public sealed class TempDrugTests
{
    [Fact]
    public void LinkToDrugForm_WhenVetDrugAlreadyLinked_ReturnsValidationError()
    {
        TempDrug tempDrug = TempDrug.CreateImported(
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

        tempDrug.LinkToVetDrug(Guid.NewGuid()).IsError.Should().BeFalse();

        ErrorOr<Success> result = tempDrug.LinkToDrugForm(Guid.NewGuid());

        result.IsError.Should().BeTrue();
        result.FirstError.Description.Should()
            .Be("LinkedDrugVetId must be null when linking to a drug form.");
    }

    [Fact]
    public void ChangeStatus_WhenSameStatus_IsIdempotent()
    {
        TempDrug tempDrug = TempDrug.CreateImported(
            productIdentifier: "101",
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

        tempDrug.Status.Should().Be(TempDrugStatus.PendingReview);

        ErrorOr<Success> result = tempDrug.ChangeStatus(TempDrugStatus.PendingReview);

        result.IsError.Should().BeFalse();
        tempDrug.Status.Should().Be(TempDrugStatus.PendingReview);
    }
}
