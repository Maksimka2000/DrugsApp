using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.ActiveSubstanceFeature.Repositories;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.DrugFeature.Repositories;
using DrugsApp.Features.DrugFeature.UpdateDrug;
using ErrorOr;
using FluentAssertions;
using Moq;

namespace DrugsApp.UnitTests.DrugFeature.UpdateDrug.Handlers;

public sealed class UpdateDrugHandlerTests
{
    [Fact]
    public async Task Handle_WhenDrugNotFound_ReturnsNotFound()
    {
        var drugId = Guid.NewGuid();
        var drugRepository = new Mock<IDrugRepository>();
        drugRepository
            .Setup(r => r.GetByIdAsync(drugId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Drug?)null);

        var handler = new UpdateDrugHandler(
            drugRepository.Object,
            new Mock<IActiveSubstanceRepository>().Object);

        var command = new UpdateDrugCommand(
            drugId,
            new UpdateDrugRequest(CommercialName: "Updated", NewActiveSubstanceIds: null, TypeOfDrug: null));

        ErrorOr<UpdatedDrugResponse> result = await handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        result.FirstError.Metadata!["resourceType"].Should().Be("Drug");
    }
}
