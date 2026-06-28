using System.Net.Http.Json;
using DrugsApp.Features.AdminUserManagementFeature.Shared;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.AdminUserManagementFeature.ChangeAdminUserLoginEmail;

public sealed class ChangeAdminUserLoginEmailReturns200Tests : IntegrationTestBase
{
    [Fact]
    public async Task ChangeAdminUserLoginEmail_ForCurrentAdmin_Returns200AndAllowsLoginWithNewEmail()
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

        string newEmail = AdminTestFlows.CreateUniqueAdminEmail();

        HttpResponseMessage changeResponse = await Client.PutAsJsonAsync(
            "/api/admin-users/me/login-email",
            new ChangeAdminUserLoginEmailRequest(newEmail),
            IntegrationTestJson.Options,
            CancellationToken.None);

        changeResponse.EnsureSuccessStatusCode();

        AdminUserDetailDto? payload = await changeResponse.Content.ReadFromJsonAsync<AdminUserDetailDto>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.Email.Should().Be(newEmail);

        TestAuthHelper.ClearBearerToken(Client);

        await TestAuthHelper.LoginAdminAsync(
            Client,
            newEmail,
            createdAdmin.Password,
            CancellationToken.None);
    }
}
