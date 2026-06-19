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

    [Fact]
    public void RemoveDrugForm_WhenLastOne_KeepsForm()
    {
        var substance = ActiveSubstance.Create("Paracetamol");
        Drug drug = Drug.Create("Apap", "Ludzki", DrugType.Standard, [substance], drugForms: null);
        DrugForm form = DrugForm.Create(drug.Id, productIdentifier: 100);
        drug.AddDrugForm(form);

        drug.RemoveDrugForm(form);

        drug.DrugForms.Should().ContainSingle(f => f.Id == form.Id);
    }

    [Fact]
    public void Create_TrimsCommercialNameAndTypeOfProduct()
    {
        Drug drug = Drug.Create(
            "  Apap  ",
            "  Ludzki  ",
            DrugType.Standard,
            activeSubstances: null,
            drugForms: null);

        drug.CommercialName.Should().Be("Apap");
        drug.TypeOfProduct.Should().Be("Ludzki");
    }
}
