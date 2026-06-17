using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.RegularDrugsFeature.Shared;
using FluentAssertions;

namespace DrugsApp.UnitTests.DrugInteractionToolFeature.Domain;

public sealed class RegularDrugEligibilityTests
{
    [Fact]
    public void IsEligibleDrug_WhenSpecialType_ReturnsFalse()
    {
        var drug = Drug.Create(
            "Special Drug",
            "Ludzki",
            DrugType.Special,
            [ActiveSubstance.Create("Substance")],
            drugForms: null);

        RegularDrugEligibility.IsEligibleDrug(drug).Should().BeFalse();
    }

    [Fact]
    public void IsEligibleDrug_WhenVeterinaryProduct_ReturnsFalse()
    {
        var drug = Drug.Create(
            "Vet Drug",
            "Weterynaryjny",
            DrugType.Standard,
            [ActiveSubstance.Create("Substance")],
            drugForms: null);

        RegularDrugEligibility.IsEligibleDrug(drug).Should().BeFalse();
    }
}
