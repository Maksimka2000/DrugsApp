using System.Net.Http.Json;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.AdminUserManagementFeature.Shared;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.AdminUserManagementFeature.CreateAdminUser;

public sealed class CreateAdminUserReturns200Tests : IntegrationTestBase
{
    [Fact]
    public async Task CreateAdminUser_WithSuperAdmin_Returns200AndGeneratedCredentials()
    {
        await AdminTestFlows.AuthenticateAsSuperAdminAsync(Client, CancellationToken.None);

        string email = AdminTestFlows.CreateUniqueAdminEmail();

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/admin-users",
            new CreateAdminUserRequest("Integration Catalog Admin", email, AuthRoles.CatalogAdmin),
            IntegrationTestJson.Options,
            CancellationToken.None);

        response.EnsureSuccessStatusCode();

        CreateAdminUserResponse? payload = await response.Content.ReadFromJsonAsync<CreateAdminUserResponse>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.Id.Should().NotBe(Guid.Empty);
        payload.Email.Should().Be(email);
        payload.AdminRole.Should().Be(AuthRoles.CatalogAdmin);
        payload.FullName.Should().Be("Integration Catalog Admin");
        payload.Password.Should().NotBeNullOrWhiteSpace();

        await TestAuthHelper.LoginAdminAsync(Client, email, payload.Password, CancellationToken.None);
    }
}
