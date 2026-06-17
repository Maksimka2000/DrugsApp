using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.DrugFeature.Entities;
using FluentAssertions;

namespace DrugsApp.UnitTests.DrugFeature.Domain;

public sealed class DrugTests
{
    [Fact]
    public void RemoveActiveSubstance_WhenLastOne_KeepsSubstance()
    {
        var substance = ActiveSubstance.Create("Paracetamol");
        var drug = Drug.Create(
            "Apap",
            "Ludzki",
            DrugType.Standard,
            [substance],
            drugForms: null);

        drug.RemoveActiveSubstance(substance);

        drug.ActiveSubstances.Should().ContainSingle(s => s.Id == substance.Id);
    }
}
