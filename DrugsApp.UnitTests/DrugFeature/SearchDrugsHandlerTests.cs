using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.DrugFeature.Repositories;
using DrugsApp.Features.DrugFeature.SearchDrugs;
using ErrorOr;
using Moq;

namespace DrugsApp.UnitTests.DrugFeature;

public sealed class SearchDrugsHandlerTests
{
    [Fact]
    public async Task Handle_Success_NoSearchQuery_ReturnsItemsAndTotalCount()
    {
        Mock<IDrugRepository> repository = new(MockBehavior.Strict);
        var drug = Drug.Create("Aspirin", "Human", DrugType.Standard, null, null);

        repository
            .Setup(r => r.SearchAsync(
                null,
                null,
                DrugSearchField.None,
                DrugSortField.CommercialName,
                true,
                0,
                11,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Drug> { drug });

        repository
            .Setup(r => r.CountAsync(null, null, DrugSearchField.None, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        SearchDrugsHandler handler = new(repository.Object);
        SearchDrugsQuery query = new(null, null, "commercial_name", "ascending", 0, 10);

        ErrorOr<SearchDrugsResponse> result = await handler.Handle(query, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Single(result.Value.Items);
        Assert.Equal(1, result.Value.TotalCount);
        Assert.False(result.Value.HasMore);
    }

    [Fact]
    public async Task Handle_Edge_SearchQueryWithExtraWhitespace_NormalizesBeforeCallingRepository()
    {
        Mock<IDrugRepository> repository = new(MockBehavior.Strict);
        var drug = Drug.Create("Aspirin", "Human", DrugType.Standard, null, null);

        repository
            .Setup(r => r.SearchAsync(
                "foo bar",
                null,
                DrugSearchField.CommercialName,
                DrugSortField.CommercialName,
                true,
                0,
                11,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Drug> { drug });

        repository
            .Setup(r => r.CountAsync("foo bar", null, DrugSearchField.CommercialName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        SearchDrugsHandler handler = new(repository.Object);
        SearchDrugsQuery query = new("  foo   bar  ", null, "commercial_name", "ascending", 0, 10);

        ErrorOr<SearchDrugsResponse> result = await handler.Handle(query, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Single(result.Value.Items);
        repository.Verify(
            r => r.SearchAsync(
                "foo bar",
                null,
                DrugSearchField.CommercialName,
                It.IsAny<DrugSortField>(),
                It.IsAny<bool>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Edge_NoMatches_ReturnsEmptyListAndZeroTotal()
    {
        Mock<IDrugRepository> repository = new(MockBehavior.Strict);

        repository
            .Setup(r => r.SearchAsync(
                null,
                null,
                DrugSearchField.None,
                DrugSortField.CommercialName,
                true,
                0,
                11,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Drug>());

        SearchDrugsHandler handler = new(repository.Object);
        SearchDrugsQuery query = new(null, null, "commercial_name", "ascending", 0, 10);

        ErrorOr<SearchDrugsResponse> result = await handler.Handle(query, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Empty(result.Value.Items);
        Assert.Equal(0, result.Value.TotalCount);
        Assert.False(result.Value.HasMore);
        Assert.Contains("without query", result.Value.Metadata.Message);
    }
}
