using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.InteractionFeature.Entities;
using DrugsApp.Domain.InteractionFeature.Enums;
using DrugsApp.Features.DrugInteractionToolFeature.Shared;
using FluentAssertions;

namespace DrugsApp.UnitTests.DrugInteractionToolFeature.Shared;

public sealed class DrugPairInteractionBuilderTests
{
    private readonly InteractionSeverity _severity = InteractionSeverity.Create(1, "High", null);

    [Fact]
    public void BuildCrossDrugPairs_WhenPairIsStoredInReverseOrder_OrientsRowsFromEachDrugPointOfView()
    {
        ActiveSubstance substanceOne = ActiveSubstance.Create("Aspirin");
        ActiveSubstance substanceTwo = ActiveSubstance.Create("Warfarin");
        DrugFormIntake first = CreateIntake("Drug One", substanceOne);
        DrugFormIntake second = CreateIntake("Drug Two", substanceTwo);
        Interaction interaction = CreateInteraction("bleeding", "stomach");

        var result = Build(
            [first, second],
            [Expanded(interaction, substanceTwo, substanceOne)],
            interaction,
            substanceOne,
            substanceTwo);

        SubstancePairInteractionRowReadModel forward = result[new CrossDrugPairKey(first.Form.Id, second.Form.Id)].Single();
        forward.CurrentActiveSubstanceName.Should().Be("Aspirin");
        forward.ComparedActiveSubstanceName.Should().Be("Warfarin");
        forward.DefaultText.Should().Be("bleeding");
        forward.Severity.Should().Be("High");

        SubstancePairInteractionRowReadModel reverse = result[new CrossDrugPairKey(second.Form.Id, first.Form.Id)].Single();
        reverse.CurrentActiveSubstanceName.Should().Be("Warfarin");
        reverse.ComparedActiveSubstanceName.Should().Be("Aspirin");
        reverse.InteractionId.Should().Be(interaction.Id);
    }

    [Fact]
    public void BuildCrossDrugPairs_WhenSameSubstancePairHasTwoInteractions_ReturnsBothRows()
    {
        ActiveSubstance substanceOne = ActiveSubstance.Create("Aspirin");
        ActiveSubstance substanceTwo = ActiveSubstance.Create("Warfarin");
        DrugFormIntake first = CreateIntake("Drug One", substanceOne);
        DrugFormIntake second = CreateIntake("Drug Two", substanceTwo);
        Interaction firstInteraction = CreateInteraction("first text", null);
        Interaction secondInteraction = CreateInteraction("second text", null);

        var result = Build(
            [first, second],
            [Expanded(firstInteraction, substanceOne, substanceTwo), Expanded(secondInteraction, substanceTwo, substanceOne)],
            [firstInteraction, secondInteraction],
            substanceOne,
            substanceTwo);

        result[new CrossDrugPairKey(first.Form.Id, second.Form.Id)]
            .Select(row => row.InteractionId)
            .Should().BeEquivalentTo([firstInteraction.Id, secondInteraction.Id]);
    }

    [Fact]
    public void BuildCrossDrugPairs_WhenBothDrugsContainBothSubstances_ReturnsTheStoredRowOnce()
    {
        ActiveSubstance substanceOne = ActiveSubstance.Create("Aspirin");
        ActiveSubstance substanceTwo = ActiveSubstance.Create("Warfarin");
        DrugFormIntake first = CreateIntake("Drug One", substanceOne, substanceTwo);
        DrugFormIntake second = CreateIntake("Drug Two", substanceOne, substanceTwo);
        Interaction interaction = CreateInteraction("text", null);

        var result = Build(
            [first, second],
            [Expanded(interaction, substanceOne, substanceTwo)],
            interaction,
            substanceOne,
            substanceTwo);

        result[new CrossDrugPairKey(first.Form.Id, second.Form.Id)].Should().ContainSingle();
        result[new CrossDrugPairKey(second.Form.Id, first.Form.Id)].Should().ContainSingle();
    }

    [Fact]
    public void BuildCrossDrugPairs_WhenFormsBelongToTheSameDrug_SkipsThatPairButKeepsPairsWithOtherDrugs()
    {
        ActiveSubstance substanceOne = ActiveSubstance.Create("Amoxicillin");
        ActiveSubstance substanceTwo = ActiveSubstance.Create("Clavulanate");
        ActiveSubstance otherSubstance = ActiveSubstance.Create("Warfarin");
        (Drug sameDrug, DrugForm firstForm, DrugForm secondForm) =
            CreateDrugWithTwoForms("Amoclan", substanceOne, substanceTwo);
        DrugFormIntake firstIntake = new(sameDrug, firstForm);
        DrugFormIntake secondIntake = new(sameDrug, secondForm);
        DrugFormIntake otherIntake = CreateIntake("Other", otherSubstance);
        Interaction intraDrug = CreateInteraction("intra-drug", null);
        Interaction withOther = CreateInteraction("with other", null);

        var result = Build(
            [firstIntake, secondIntake, otherIntake],
            [Expanded(intraDrug, substanceOne, substanceTwo), Expanded(withOther, substanceOne, otherSubstance)],
            [intraDrug, withOther],
            substanceOne,
            substanceTwo,
            otherSubstance);

        result.Should().NotContainKey(new CrossDrugPairKey(firstForm.Id, secondForm.Id));
        result.Should().NotContainKey(new CrossDrugPairKey(secondForm.Id, firstForm.Id));
        result[new CrossDrugPairKey(firstForm.Id, otherIntake.Form.Id)].Should().ContainSingle();
        result[new CrossDrugPairKey(secondForm.Id, otherIntake.Form.Id)].Should().ContainSingle();
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, false, false)]
    [InlineData(false, false, false)]
    public void BuildCrossDrugPairs_AttachesStomachTextOnlyWhenBothFormsAreOral(
        bool firstIsOral,
        bool secondIsOral,
        bool expectStomachText)
    {
        ActiveSubstance substanceOne = ActiveSubstance.Create("Aspirin");
        ActiveSubstance substanceTwo = ActiveSubstance.Create("Warfarin");
        DrugFormIntake first = CreateIntake("Drug One", substanceOne);
        DrugFormIntake second = CreateIntake("Drug Two", substanceTwo);
        SetOral(first.Form, firstIsOral);
        SetOral(second.Form, secondIsOral);
        Interaction interaction = CreateInteraction("text", "take with food");

        var result = Build(
            [first, second],
            [Expanded(interaction, substanceOne, substanceTwo)],
            interaction,
            substanceOne,
            substanceTwo);

        string? stomachText = result[new CrossDrugPairKey(first.Form.Id, second.Form.Id)].Single().StomachText;
        stomachText.Should().Be(expectStomachText ? "take with food" : null);
    }

    [Fact]
    public void BuildCrossDrugPairs_WhenNoRowsOrFewerThanTwoDrugs_ReturnsEmpty()
    {
        ActiveSubstance substance = ActiveSubstance.Create("Aspirin");
        DrugFormIntake only = CreateIntake("Drug One", substance);

        DrugPairInteractionBuilder.BuildCrossDrugPairs(
                [only],
                [],
                new Dictionary<Guid, Interaction>(),
                new Dictionary<Guid, string>())
            .Should().BeEmpty();
    }

    [Fact]
    public void BuildCrossDrugPairs_WhenParentInteractionIsMissing_SkipsTheRow()
    {
        ActiveSubstance substanceOne = ActiveSubstance.Create("Aspirin");
        ActiveSubstance substanceTwo = ActiveSubstance.Create("Warfarin");
        DrugFormIntake first = CreateIntake("Drug One", substanceOne);
        DrugFormIntake second = CreateIntake("Drug Two", substanceTwo);
        Interaction interaction = CreateInteraction("text", null);

        var result = DrugPairInteractionBuilder.BuildCrossDrugPairs(
            [first, second],
            [Expanded(interaction, substanceOne, substanceTwo)],
            new Dictionary<Guid, Interaction>(),
            NamesOf(substanceOne, substanceTwo));

        result[new CrossDrugPairKey(first.Form.Id, second.Form.Id)].Should().BeEmpty();
    }

    private Dictionary<CrossDrugPairKey, List<SubstancePairInteractionRowReadModel>> Build(
        IReadOnlyList<DrugFormIntake> intakes,
        IReadOnlyCollection<InteractionExpanded> rows,
        Interaction interaction,
        params ActiveSubstance[] substances) =>
        Build(intakes, rows, [interaction], substances);

    private static Dictionary<CrossDrugPairKey, List<SubstancePairInteractionRowReadModel>> Build(
        IReadOnlyList<DrugFormIntake> intakes,
        IReadOnlyCollection<InteractionExpanded> rows,
        IReadOnlyCollection<Interaction> interactions,
        params ActiveSubstance[] substances) =>
        DrugPairInteractionBuilder.BuildCrossDrugPairs(
            intakes,
            rows,
            interactions.ToDictionary(interaction => interaction.Id),
            NamesOf(substances));

    private static Dictionary<Guid, string> NamesOf(params ActiveSubstance[] substances) =>
        substances.ToDictionary(substance => substance.Id, substance => substance.ActiveSubstanceName);

    private static InteractionExpanded Expanded(Interaction interaction, ActiveSubstance partyA, ActiveSubstance partyB) =>
        InteractionExpanded.Create(
            interaction.Id,
            ShortInteractionType.Substance,
            partyA.Id,
            ShortInteractionType.Substance,
            partyB.Id);

    private Interaction CreateInteraction(string defaultText, string? stomachText)
    {
        Interaction interaction = Interaction.Create(_severity.Id, defaultText, stomachText).Value;
        typeof(Interaction).GetProperty(nameof(Interaction.InteractionSeverity))!.SetValue(interaction, _severity);
        return interaction;
    }

    private static DrugFormIntake CreateIntake(string name, params ActiveSubstance[] substances)
    {
        Drug drug = Drug.Create(name, "Ludzki", DrugType.Standard, substances, drugForms: null);
        DrugForm form = DrugForm.Create(drug.Id, productIdentifier: 1, dosage: "10 mg");
        AttachOwningDrug(form, drug);
        drug.AddDrugForm(form);
        return new DrugFormIntake(drug, form);
    }

    private static (Drug Drug, DrugForm FirstForm, DrugForm SecondForm) CreateDrugWithTwoForms(
        string name,
        params ActiveSubstance[] substances)
    {
        Drug drug = Drug.Create(name, "Ludzki", DrugType.Standard, substances, drugForms: null);
        DrugForm firstForm = DrugForm.Create(drug.Id, productIdentifier: 1, dosage: "10 mg");
        DrugForm secondForm = DrugForm.Create(drug.Id, productIdentifier: 2, dosage: "20 mg");
        AttachOwningDrug(firstForm, drug);
        AttachOwningDrug(secondForm, drug);
        drug.AddDrugForm(firstForm);
        drug.AddDrugForm(secondForm);
        return (drug, firstForm, secondForm);
    }

    private static void SetOral(DrugForm form, bool isOral)
    {
        typeof(DrugForm).GetProperty(nameof(DrugForm.IsOral))!.SetValue(form, isOral);
    }

    private static void AttachOwningDrug(DrugForm form, Drug drug)
    {
        typeof(DrugForm).GetProperty(nameof(DrugForm.Drug))!.SetValue(form, drug);
    }
}
