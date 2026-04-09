using DrugsApp.Domain.RouteOfAdministrationFeature.Entities;
using DrugsApp.Domain.RouteOfAdministrationFeature.Repositories;
using DrugsApp.Features.RouteOfAdministrationFeature.AddRouteOfAdministration;
using ErrorOr;
using Moq;

namespace DrugsApp.UnitTests.RouteOfAdministrationFeature;

public sealed class AddRouteOfAdministrationHandlerTests
{
    [Fact]
    public async Task Handle_Success_WhenNameUnique_PersistsAndReturnsCreatedResponse()
    {
        Mock<IRouteOfAdministrationRepository> repository = new(MockBehavior.Strict);
        RouteOfAdministration? added = null;

        repository
            .Setup(r => r.GetByNameAsync("Oral", It.IsAny<CancellationToken>()))
            .ReturnsAsync((RouteOfAdministration?)null);

        repository
            .Setup(r => r.AddAsync(It.IsAny<RouteOfAdministration>(), It.IsAny<CancellationToken>()))
            .Callback<RouteOfAdministration, CancellationToken>((entity, _) => added = entity)
            .Returns(Task.CompletedTask);

        AddRouteOfAdministrationHandler handler = new(repository.Object);
        AddRouteOfAdministrationCommand command = new(new AddRouteOfAdministrationRequest("  Oral  ", true));

        ErrorOr<CreatedRouteOfAdministrationResponse> result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.NotNull(added);
        Assert.Equal("Oral", added!.RouteOfAdministrationName);
        Assert.True(added.IsOralIntake);
        Assert.Equal(added.Id, result.Value.Id);
    }

    [Fact]
    public async Task Handle_Failure_WhenNameExists_ReturnsConflict()
    {
        Mock<IRouteOfAdministrationRepository> repository = new(MockBehavior.Strict);
        var existing = RouteOfAdministration.Create("Oral");

        repository
            .Setup(r => r.GetByNameAsync("Oral", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        AddRouteOfAdministrationHandler handler = new(repository.Object);
        AddRouteOfAdministrationCommand command = new(new AddRouteOfAdministrationRequest("Oral", true));

        ErrorOr<CreatedRouteOfAdministrationResponse> result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal(ErrorType.Conflict, result.FirstError.Type);
        repository.Verify(r => r.AddAsync(It.IsAny<RouteOfAdministration>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Edge_TrimmedNameCollidesWithExistingNormalizedName_ReturnsConflict()
    {
        Mock<IRouteOfAdministrationRepository> repository = new(MockBehavior.Strict);
        var existing = RouteOfAdministration.Create("Oral");

        repository
            .Setup(r => r.GetByNameAsync("Oral", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        AddRouteOfAdministrationHandler handler = new(repository.Object);
        AddRouteOfAdministrationCommand command = new(new AddRouteOfAdministrationRequest("  Oral  ", false));

        ErrorOr<CreatedRouteOfAdministrationResponse> result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal(ErrorType.Conflict, result.FirstError.Type);
    }
}
