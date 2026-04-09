using DrugsApp.Domain.TempDrugFeature.Entities;
using DrugsApp.Domain.TempDrugFeature.Repositories;
using DrugsApp.Features.TempDrugFeature.SearchTempDrugs;
using ErrorOr;
using Moq;

namespace DrugsApp.UnitTests.TempDrugFeature;

public sealed class SearchTempDrugsHandlerTests
{
    [Fact]
    public async Task Handle_Success_FirstPage_ReturnsItemsTotalCountAndHasMoreWhenExtraRowFetched()
    {
        Mock<ITempDrugRepository> repository = new(MockBehavior.Strict);
        var batchId = Guid.NewGuid();
        var pagePlusOne = Enumerable.Range(0, 11).Select(SeedTempDrug(batchId)).ToList();

        repository
            .Setup(r => r.SearchAsync(
                null,
                null,
                null,
                TempDrugSortField.CreatedAtUtc,
                false,
                0,
                11,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagePlusOne);

        repository
            .Setup(r => r.CountAsync(null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(50);

        SearchTempDrugsHandler handler = new(repository.Object);
        SearchTempDrugsQuery query = new(null, null, null, "created_at", "descending", 0, 10);

        ErrorOr<SearchTempDrugsResponse> result = await handler.Handle(query, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Equal(10, result.Value.Items.Count);
        Assert.True(result.Value.HasMore);
        Assert.Equal(50, result.Value.TotalCount);
    }

    [Fact]
    public async Task Handle_Edge_CommercialNameTrimsWhitespace_PassesNormalizedNameToRepository()
    {
        Mock<ITempDrugRepository> repository = new(MockBehavior.Strict);

        repository
            .Setup(r => r.SearchAsync(
                "Aspirin",
                null,
                null,
                TempDrugSortField.CreatedAtUtc,
                false,
                0,
                11,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TempDrug>());

        repository
            .Setup(r => r.CountAsync("Aspirin", null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        SearchTempDrugsHandler handler = new(repository.Object);
        SearchTempDrugsQuery query = new("  Aspirin  ", null, null, "created_at", "descending", 0, 10);

        ErrorOr<SearchTempDrugsResponse> result = await handler.Handle(query, CancellationToken.None);

        Assert.False(result.IsError);
        repository.Verify(
            r => r.SearchAsync("Aspirin", It.IsAny<IReadOnlyCollection<TempDrugStatus>?>(), It.IsAny<IReadOnlyCollection<string>?>(), It.IsAny<TempDrugSortField>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Edge_NoRows_ReturnsEmptyPageAndCountFromRepositoryOnFirstPage()
    {
        Mock<ITempDrugRepository> repository = new(MockBehavior.Strict);

        repository
            .Setup(r => r.SearchAsync(
                null,
                null,
                null,
                TempDrugSortField.CreatedAtUtc,
                false,
                0,
                11,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TempDrug>());

        repository
            .Setup(r => r.CountAsync(null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        SearchTempDrugsHandler handler = new(repository.Object);
        SearchTempDrugsQuery query = new(null, null, null, "created_at", "descending", 0, 10);

        ErrorOr<SearchTempDrugsResponse> result = await handler.Handle(query, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Empty(result.Value.Items);
        Assert.Equal(0, result.Value.TotalCount);
        Assert.False(result.Value.HasMore);
    }

    private static Func<int, TempDrug> SeedTempDrug(Guid batchId) => i =>
    {
        ErrorOr<TempDrug> created = TempDrug.CreateImported(
            $"pid-{i}",
            $"Name-{i}",
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            batchId);
        Assert.False(created.IsError);
        return created.Value;
    };
}
