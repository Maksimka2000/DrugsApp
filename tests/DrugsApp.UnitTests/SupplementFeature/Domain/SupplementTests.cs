using DrugsApp.Domain.SupplementFeature.Entities;
using FluentAssertions;

namespace DrugsApp.UnitTests.SupplementFeature.Domain;

public sealed class SupplementTests
{
    [Fact]
    public void Create_BuildsSearchTextFromNameAndAliases()
    {
        Supplement supplement = Supplement.Create("Omega-3 Fish Oil", ["EPA", "DHA"]);

        supplement.SearchText.Should().Be("omega-3 fish oil epa dha");
    }

    [Fact]
    public void Create_BuildsSearchOutputTextWithAliasesInParentheses()
    {
        Supplement supplement = Supplement.Create("Omega-3 Fish Oil", ["EPA", "DHA"]);

        supplement.BuildSearchOutputText().Should().Be("omega-3 fish oil (epa, dha)");
    }

    [Fact]
    public void Create_BuildsSearchOutputTextWithoutParenthesesWhenNoAliases()
    {
        Supplement supplement = Supplement.Create("Zinc", []);

        supplement.BuildSearchOutputText().Should().Be("zinc");
    }

    [Fact]
    public void ReplaceAliases_RebuildsSearchText()
    {
        Supplement supplement = Supplement.Create("Omega-3 Fish Oil", ["EPA"]);

        supplement.ReplaceAliases(["EPA", "DHA"]);

        supplement.SearchText.Should().Be("omega-3 fish oil epa dha");
        supplement.BuildSearchOutputText().Should().Be("omega-3 fish oil (epa, dha)");
    }

    [Fact]
    public void UpdateName_RebuildsSearchText()
    {
        Supplement supplement = Supplement.Create("Iron", ["ferrous sulfate"]);

        supplement.UpdateName("Iron supplement");

        supplement.Name.Should().Be("Iron supplement");
        supplement.SearchText.Should().Be("iron supplement ferrous sulfate");
    }
}
