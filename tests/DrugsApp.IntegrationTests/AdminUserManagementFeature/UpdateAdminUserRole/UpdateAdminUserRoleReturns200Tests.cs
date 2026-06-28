using System.Net.Http.Json;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.AdminUserManagementFeature.Shared;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.AdminUserManagementFeature.UpdateAdminUserRole;

public sealed class UpdateAdminUserRoleReturns200Tests : IntegrationTestBase
{
    [Fact]
    public async Task UpdateAdminUserRole_WithCreatedAdmin_Returns200AndUpdatedRole()
    {
        await AdminTestFlows.AuthenticateAsSuperAdminAsync(Client, CancellationToken.None);

        CreateAdminUserResponse createdAdmin = await AdminTestFlows.CreateAdminUserAsync(
            Client,
            CancellationToken.None,
            adminRole: AuthRoles.CatalogAdmin);

        HttpResponseMessage updateResponse = await Client.PutAsJsonAsync(
            $"/api/admin-users/{createdAdmin.Id}/role",
            new UpdateAdminUserRoleRequest(AuthRoles.ContentAdmin),
            IntegrationTestJson.Options,
            CancellationToken.None);

        updateResponse.EnsureSuccessStatusCode();

        AdminUserDetailDto? payload = await updateResponse.Content.ReadFromJsonAsync<AdminUserDetailDto>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.Id.Should().Be(createdAdmin.Id);
        payload.AdminRole.Should().Be(AuthRoles.ContentAdmin);
        payload.Email.Should().Be(createdAdmin.Email);
    }
}
