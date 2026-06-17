using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.DrugFeature.DomainServices;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.PharmaceuticalFormFeature.Entities;
using DrugsApp.Domain.RouteOfAdministrationFeature.Entities;
using ErrorOr;
using FluentAssertions;

namespace DrugsApp.UnitTests.DrugFeature.Domain;

public sealed class DrugSimilarityDomainServiceTests
{
    private readonly DrugSimilarityDomainService _service = new();

    [Fact]
    public void ScoreAndClassify_WhenAllFieldsMatch_ReturnsDuplication()
    {
        (Drug drug, DrugSimilaritySnapshot snapshot) = BuildCandidate(
            commercialName: "Apap",
            typeOfProduct: "Ludzki",
            atc: "N02BE01",
            dosage: "500mg",
            leaflet: "http://leaflet",
            education: "http://edu");

        ErrorOr<DrugSimilarityResult> result = _service.ScoreAndClassify(snapshot, drug);

        result.IsError.Should().BeFalse();
        result.Value.SimilarityClassification.Should().Be(DrugSimilarityClassification.Duplication);
        result.Value.MatchScorePercent.Should().Be(100);
        result.Value.IsMergeAllowed.Should().BeFalse();
        result.Value.IsUpdateAllowed.Should().BeFalse();
    }

    [Fact]
    public void ScoreAndClassify_WhenOnlyLinksDiffer_ReturnsUpdate()
    {
        (Drug drug, DrugSimilaritySnapshot snapshot) = BuildCandidate(
            commercialName: "Apap",
            typeOfProduct: "Ludzki",
            atc: "N02BE01",
            dosage: "500mg",
            leaflet: "http://old-leaflet",
            education: "http://old-edu");

        DrugSimilaritySnapshot reviewSnapshot = snapshot with
        {
            Leaflet = "http://new-leaflet",
            EducationalMaterials = "http://new-edu"
        };

        ErrorOr<DrugSimilarityResult> result = _service.ScoreAndClassify(reviewSnapshot, drug);

        result.IsError.Should().BeFalse();
        result.Value.SimilarityClassification.Should().Be(DrugSimilarityClassification.Update);
        result.Value.IsUpdateAllowed.Should().BeTrue();
        result.Value.IsMergeAllowed.Should().BeFalse();
    }

    [Fact]
    public void ScoreAndClassify_WhenAtcDiffers_ReturnsMerge()
    {
        (Drug drug, DrugSimilaritySnapshot snapshot) = BuildCandidate(
            commercialName: "Apap",
            typeOfProduct: "Ludzki",
            atc: "N02BE01",
            dosage: "500mg",
            leaflet: "http://leaflet",
            education: "http://edu");

        DrugSimilaritySnapshot reviewSnapshot = snapshot with { AtcCode = "N02BE99" };

        ErrorOr<DrugSimilarityResult> result = _service.ScoreAndClassify(reviewSnapshot, drug);

        result.IsError.Should().BeFalse();
        result.Value.SimilarityClassification.Should().Be(DrugSimilarityClassification.Merge);
        result.Value.IsMergeAllowed.Should().BeTrue();
    }

    [Fact]
    public void ScoreAndClassify_WhenCommercialNameDiffers_ReturnsNameOnly()
    {
        (Drug drug, DrugSimilaritySnapshot snapshot) = BuildCandidate(
            commercialName: "Apap",
            typeOfProduct: "Ludzki",
            atc: "N02BE01",
            dosage: "500mg",
            leaflet: "http://leaflet",
            education: "http://edu");

        DrugSimilaritySnapshot reviewSnapshot = snapshot with { CommercialName = "Different Name" };

        ErrorOr<DrugSimilarityResult> result = _service.ScoreAndClassify(reviewSnapshot, drug);

        result.IsError.Should().BeFalse();
        result.Value.SimilarityClassification.Should().Be(DrugSimilarityClassification.NameOnly);
        result.Value.IsMergeAllowed.Should().BeFalse();
        result.Value.IsUpdateAllowed.Should().BeFalse();
    }

    private static (Drug Drug, DrugSimilaritySnapshot Snapshot) BuildCandidate(
        string commercialName,
        string typeOfProduct,
        string atc,
        string dosage,
        string leaflet,
        string education)
    {
        ActiveSubstance substance = ActiveSubstance.Create("Paracetamol");
        RouteOfAdministration route = RouteOfAdministration.Create("Doustna");
        PharmaceuticalForm formType = PharmaceuticalForm.Create("Tabletka");

        Drug drug = Drug.Create(commercialName, typeOfProduct, DrugType.Standard, [substance], drugForms: null);
        DrugForm drugForm = DrugForm.Create(
            drug.Id,
            productIdentifier: 12345,
            atcCode: atc,
            dosage: dosage,
            leaflet: leaflet,
            educationMaterials: education);
        drugForm.AddRouteOfAdministration(route);
        drugForm.AddPharmaceuticalForm(formType);
        drug.AddDrugForm(drugForm);

        var snapshot = new DrugSimilaritySnapshot(
            CommercialName: commercialName,
            TypeOfProduct: typeOfProduct,
            ActiveSubstanceIds: [substance.Id],
            AtcCode: atc,
            Dosage: dosage,
            Leaflet: leaflet,
            EducationalMaterials: education,
            RouteOfAdministrationIds: [route.Id],
            PharmaceuticalFormIds: [formType.Id]);

        return (drug, snapshot);
    }
}
