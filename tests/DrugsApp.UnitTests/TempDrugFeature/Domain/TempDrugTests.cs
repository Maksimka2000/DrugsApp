using DrugsApp.Domain.TempDrugFeature.Entities;
using ErrorOr;
using FluentAssertions;

namespace DrugsApp.UnitTests.TempDrugFeature.Domain;

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

    [Fact]
    public void SetNote_TrimsWhitespace()
    {
        TempDrug tempDrug = TempDrug.CreateImported(
            productIdentifier: "102",
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

        tempDrug.SetNote("  reviewed note  ").IsError.Should().BeFalse();

        tempDrug.Note.Should().Be("reviewed note");
    }

    [Fact]
    public void CreateImported_WhenProductIdentifierEmpty_ReturnsValidationError()
    {
        ErrorOr<TempDrug> result = TempDrug.CreateImported(
            productIdentifier: "  ",
            commercialName: "Apap",
            activeSubstance: null,
            typeOfProduct: null,
            routeOfAdministration: null,
            pharmaceuticalFormulation: null,
            atcCode: null,
            leaflet: null,
            educationalMaterials: null,
            dosage: null,
            linkedImportId: Guid.NewGuid());

        result.IsError.Should().BeTrue();
        result.FirstError.Description.Should().Be("ProductIdentifier is required.");
    }

    [Fact]
    public void CreateImported_WhenLinkedImportIdEmpty_ReturnsValidationError()
    {
        ErrorOr<TempDrug> result = TempDrug.CreateImported(
            productIdentifier: "100",
            commercialName: "Apap",
            activeSubstance: null,
            typeOfProduct: null,
            routeOfAdministration: null,
            pharmaceuticalFormulation: null,
            atcCode: null,
            leaflet: null,
            educationalMaterials: null,
            dosage: null,
            linkedImportId: Guid.Empty);

        result.IsError.Should().BeTrue();
        result.FirstError.Description.Should().Be("LinkedImportId must not be empty.");
    }

    [Fact]
    public void LinkToVetDrug_WhenDrugFormAlreadyLinked_ReturnsValidationError()
    {
        TempDrug tempDrug = TempDrug.CreateImported(
            productIdentifier: "103",
            commercialName: "Apap",
            activeSubstance: "Paracetamol",
            typeOfProduct: "Weterynaryjny",
            routeOfAdministration: null,
            pharmaceuticalFormulation: null,
            atcCode: null,
            leaflet: null,
            educationalMaterials: null,
            dosage: null,
            linkedImportId: Guid.NewGuid()).Value;

        tempDrug.LinkToDrugForm(Guid.NewGuid()).IsError.Should().BeFalse();

        ErrorOr<Success> result = tempDrug.LinkToVetDrug(Guid.NewGuid());

        result.IsError.Should().BeTrue();
        result.FirstError.Description.Should()
            .Be("LinkedDrugFormId must be null when linking to a veterinary drug.");
    }

    [Fact]
    public void LinkToDrugForm_WhenDrugFormIdEmpty_ReturnsValidationError()
    {
        TempDrug tempDrug = TempDrug.CreateImported(
            productIdentifier: "104",
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

        ErrorOr<Success> result = tempDrug.LinkToDrugForm(Guid.Empty);

        result.IsError.Should().BeTrue();
        result.FirstError.Description.Should().Be("DrugFormId must not be empty.");
    }

    [Fact]
    public void CreateImported_TrimsProductIdentifier()
    {
        ErrorOr<TempDrug> result = TempDrug.CreateImported(
            productIdentifier: "  105  ",
            commercialName: " Apap ",
            activeSubstance: null,
            typeOfProduct: null,
            routeOfAdministration: null,
            pharmaceuticalFormulation: null,
            atcCode: null,
            leaflet: null,
            educationalMaterials: null,
            dosage: null,
            linkedImportId: Guid.NewGuid());

        result.IsError.Should().BeFalse();
        result.Value.ProductIdentifier.Should().Be("105");
        result.Value.CommercialName.Should().Be("Apap");
        result.Value.Status.Should().Be(TempDrugStatus.PendingReview);
    }
}
