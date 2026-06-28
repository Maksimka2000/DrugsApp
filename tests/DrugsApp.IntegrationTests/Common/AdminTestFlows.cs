using System.Net.Http.Json;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.AdminUserManagementFeature.Shared;
using DrugsApp.Features.AuthFeature.LoginAdmin;

namespace DrugsApp.IntegrationTests.Common;

internal static class AdminTestFlows
{
    internal static async Task AuthenticateAsSuperAdminAsync(
        HttpClient client,
        CancellationToken cancellationToken)
    {
        LoginAdminResponse login = await TestAuthHelper.LoginAsAdminAsync(client, cancellationToken);
        TestAuthHelper.ApplyBearerToken(client, login.AccessToken);
    }

    internal static async Task AuthenticateAsAdminAsync(
        HttpClient client,
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        LoginAdminResponse login = await TestAuthHelper.LoginAdminAsync(
            client,
            email,
            password,
            cancellationToken);

        TestAuthHelper.ApplyBearerToken(client, login.AccessToken);
    }

    internal static async Task<CreateAdminUserResponse> CreateAdminUserAsync(
        HttpClient client,
        CancellationToken cancellationToken,
        string? email = null,
        string adminRole = AuthRoles.CatalogAdmin)
    {
        email ??= CreateUniqueAdminEmail();

        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/admin-users",
            new CreateAdminUserRequest("Integration Admin", email, adminRole),
            IntegrationTestJson.Options,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        CreateAdminUserResponse? payload = await response.Content.ReadFromJsonAsync<CreateAdminUserResponse>(
            IntegrationTestJson.Options,
            cancellationToken);

        return payload ?? throw new InvalidOperationException("Create admin user response body was empty.");
    }

    internal static string CreateUniqueAdminEmail() => $"integration-admin-{Guid.NewGuid():N}@test.local";
}
