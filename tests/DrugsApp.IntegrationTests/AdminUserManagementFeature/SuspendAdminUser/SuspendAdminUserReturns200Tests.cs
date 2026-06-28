using System.Net;
using System.Net.Http.Json;
using DrugsApp.Features.AdminUserManagementFeature.Shared;
using DrugsApp.Features.AuthFeature.LoginAdmin;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.AdminUserManagementFeature.SuspendAdminUser;

public sealed class SuspendAdminUserReturns200Tests : IntegrationTestBase
{
    [Fact]
    public async Task SuspendAdminUser_WithCreatedAdmin_Returns200AndPreventsLogin()
    {
        await AdminTestFlows.AuthenticateAsSuperAdminAsync(Client, CancellationToken.None);

        CreateAdminUserResponse createdAdmin = await AdminTestFlows.CreateAdminUserAsync(
            Client,
            CancellationToken.None);

        HttpResponseMessage suspendResponse = await Client.PutAsJsonAsync(
            $"/api/admin-users/{createdAdmin.Id}/suspension",
            new SuspendAdminUserRequest(true),
            IntegrationTestJson.Options,
            CancellationToken.None);

        suspendResponse.EnsureSuccessStatusCode();

        AdminUserSuspensionResponse? payload = await suspendResponse.Content.ReadFromJsonAsync<AdminUserSuspensionResponse>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.Id.Should().Be(createdAdmin.Id);
        payload.Status.Should().Be("Suspended");

        TestAuthHelper.ClearBearerToken(Client);

        HttpResponseMessage loginResponse = await Client.PostAsJsonAsync(
            "/api/auth/admin/login",
            new LoginAdminRequest(createdAdmin.Email, createdAdmin.Password),
            IntegrationTestJson.Options,
            CancellationToken.None);

        loginResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
