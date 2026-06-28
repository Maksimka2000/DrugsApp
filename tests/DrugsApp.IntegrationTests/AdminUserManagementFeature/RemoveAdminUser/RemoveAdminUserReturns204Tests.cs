using System.Net;
using System.Net.Http.Json;
using DrugsApp.Features.AdminUserManagementFeature.Shared;
using DrugsApp.Features.AuthFeature.LoginAdmin;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.AdminUserManagementFeature.RemoveAdminUser;

public sealed class RemoveAdminUserReturns204Tests : IntegrationTestBase
{
    [Fact]
    public async Task RemoveAdminUser_WithCreatedAdmin_Returns204AndPreventsFutureLogin()
    {
        await AdminTestFlows.AuthenticateAsSuperAdminAsync(Client, CancellationToken.None);

        CreateAdminUserResponse createdAdmin = await AdminTestFlows.CreateAdminUserAsync(
            Client,
            CancellationToken.None);

        HttpResponseMessage removeResponse = await Client.DeleteAsync(
            $"/api/admin-users/{createdAdmin.Id}",
            CancellationToken.None);

        removeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        TestAuthHelper.ClearBearerToken(Client);

        HttpResponseMessage loginResponse = await Client.PostAsJsonAsync(
            "/api/auth/admin/login",
            new LoginAdminRequest(createdAdmin.Email, createdAdmin.Password),
            IntegrationTestJson.Options,
            CancellationToken.None);

        loginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
