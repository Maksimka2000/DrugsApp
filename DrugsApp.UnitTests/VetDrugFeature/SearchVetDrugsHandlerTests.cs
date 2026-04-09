using DrugsApp.Domain.VetDrugFeature.Entities;
using DrugsApp.Domain.VetDrugFeature.Repositories;
using DrugsApp.Features.VetDrugFeature.SearchVetDrugs;
using DrugsApp.Features.VetDrugFeature.Shared;
using ErrorOr;
using Moq;

namespace DrugsApp.UnitTests.VetDrugFeature;

public sealed class SearchVetDrugsHandlerTests
{
    [Fact]
    public async Task Handle_Success_ReturnsMappedDtos()
    {
        Mock<IVetDrugRepository> repository = new(MockBehavior.Strict);
        var drug = VetDrug.Create(1, "Rimadyl", "Vet");

        repository
            .Setup(r => r.SearchAsync("Rim", true, 0, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<VetDrug> { drug });

        SearchVetDrugsHandler handler = new(repository.Object);
        SearchVetDrugsQuery query = new("Rim", "ascending", 0, 10);

        ErrorOr<List<VetDrugDto>> result = await handler.Handle(query, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Single(result.Value);
        Assert.Equal("Rimadyl", result.Value[0].CommercialName);
    }

    [Fact]
    public async Task Handle_Edge_Descending_PassesFalseForAscendingToRepository()
    {
        Mock<IVetDrugRepository> repository = new(MockBehavior.Strict);

        repository
            .Setup(r => r.SearchAsync(null, false, 0, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<VetDrug>());

        SearchVetDrugsHandler handler = new(repository.Object);
        SearchVetDrugsQuery query = new(null, "descending", 0, 10);

        ErrorOr<List<VetDrugDto>> result = await handler.Handle(query, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Empty(result.Value);
        repository.Verify(
            r => r.SearchAsync(null, false, 0, 10, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Edge_NoMatches_ReturnsEmptyList()
    {
        Mock<IVetDrugRepository> repository = new(MockBehavior.Strict);

        repository
            .Setup(r => r.SearchAsync(null, true, 0, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<VetDrug>());

        SearchVetDrugsHandler handler = new(repository.Object);
        SearchVetDrugsQuery query = new(null, "ascending", 0, 10);

        ErrorOr<List<VetDrugDto>> result = await handler.Handle(query, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Empty(result.Value);
    }
}
