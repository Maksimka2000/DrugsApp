using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.RouteOfAdministrationFeature.Entities;
using FluentAssertions;

namespace DrugsApp.UnitTests.DrugFeature.Domain;

public sealed class DrugFormTests
{
    [Fact]
    public void SetProductIdentifier_WhenZeroOrNegative_IsNoOp()
    {
        var drugId = Guid.NewGuid();
        DrugForm form = DrugForm.Create(drugId, productIdentifier: 100);

        form.SetProductIdentifier(0);
        form.SetProductIdentifier(-5);

        form.ProductIdentifier.Should().Be(100);
    }

    [Fact]
    public void AddRouteOfAdministration_WhenOralRoute_SetsIsOralTrue()
    {
        var drugId = Guid.NewGuid();
        DrugForm form = DrugForm.Create(drugId, productIdentifier: 100);
        RouteOfAdministration oralRoute = RouteOfAdministration.Create("Doustna");
        oralRoute.SetIsOralIntake(true);

        form.AddRouteOfAdministration(oralRoute);

        form.IsOral.Should().BeTrue();
    }

    [Fact]
    public void RemoveRouteOfAdministration_RecalculatesIsOralToFalse()
    {
        var drugId = Guid.NewGuid();
        DrugForm form = DrugForm.Create(drugId, productIdentifier: 100);
        RouteOfAdministration oralRoute = RouteOfAdministration.Create("Doustna");
        oralRoute.SetIsOralIntake(true);
        form.AddRouteOfAdministration(oralRoute);

        form.RemoveRouteOfAdministration(oralRoute);

        form.IsOral.Should().BeFalse();
    }
}
