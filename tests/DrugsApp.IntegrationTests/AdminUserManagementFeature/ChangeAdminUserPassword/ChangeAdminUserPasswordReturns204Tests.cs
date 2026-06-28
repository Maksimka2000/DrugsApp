using System.Net;
using System.Net.Http.Json;
using DrugsApp.Features.AdminUserManagementFeature.Shared;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.AdminUserManagementFeature.ChangeAdminUserPassword;

public sealed class ChangeAdminUserPasswordReturns204Tests : IntegrationTestBase
{
    [Fact]
    public async Task ChangeAdminUserPassword_ForCurrentAdmin_Returns204AndAllowsLoginWithNewPassword()
    {
        await AdminTestFlows.AuthenticateAsSuperAdminAsync(Client, CancellationToken.None);

        CreateAdminUserResponse createdAdmin = await AdminTestFlows.CreateAdminUserAsync(
            Client,
            CancellationToken.None);

        TestAuthHelper.ClearBearerToken(Client);
        await AdminTestFlows.AuthenticateAsAdminAsync(
            Client,
            createdAdmin.Email,
            createdAdmin.Password,
            CancellationToken.None);

        const string newPassword = "NewAdminPass123!";

        HttpResponseMessage changeResponse = await Client.PutAsJsonAsync(
            "/api/admin-users/me/password",
            new ChangeAdminUserPasswordRequest(newPassword, newPassword),
            IntegrationTestJson.Options,
            CancellationToken.None);

        changeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        TestAuthHelper.ClearBearerToken(Client);

        await TestAuthHelper.LoginAdminAsync(
            Client,
            createdAdmin.Email,
            newPassword,
            CancellationToken.None);
    }
}
