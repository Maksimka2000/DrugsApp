using System.Net.Http.Headers;
using System.Net.Http.Json;
using DrugsApp.Features.AuthFeature.LoginAdmin;
using DrugsApp.Features.AuthFeature.LoginMember;

namespace DrugsApp.IntegrationTests.Common;

public static class TestAuthHelper
{
    public static async Task<LoginMemberResponse> LoginAsClientAsync(
        HttpClient client,
        CancellationToken cancellationToken)
    {
        return await LoginMemberAsync(
            client,
            TestCredentials.Client1Email,
            TestCredentials.Client1Password,
            cancellationToken);
    }

    public static async Task<LoginMemberResponse> LoginAsProviderAsync(
        HttpClient client,
        CancellationToken cancellationToken)
    {
        return await LoginMemberAsync(
            client,
            TestCredentials.Provider1Email,
            TestCredentials.Provider1Password,
            cancellationToken);
    }

    public static async Task<LoginMemberResponse> LoginMemberAsync(
        HttpClient client,
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginMemberRequest(email, password),
            IntegrationTestJson.Options,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        LoginMemberResponse? payload = await response.Content.ReadFromJsonAsync<LoginMemberResponse>(
            IntegrationTestJson.Options,
            cancellationToken);

        return payload ?? throw new InvalidOperationException("Login response body was empty.");
    }

    public static async Task<LoginAdminResponse> LoginAsAdminAsync(
        HttpClient client,
        CancellationToken cancellationToken)
    {
        return await LoginAdminAsync(
            client,
            TestCredentials.AdminEmail,
            TestCredentials.AdminPassword,
            cancellationToken);
    }

    public static async Task<LoginAdminResponse> LoginAdminAsync(
        HttpClient client,
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/auth/admin/login",
            new LoginAdminRequest(email, password),
            IntegrationTestJson.Options,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        LoginAdminResponse? payload = await response.Content.ReadFromJsonAsync<LoginAdminResponse>(
            IntegrationTestJson.Options,
            cancellationToken);

        return payload ?? throw new InvalidOperationException("Admin login response body was empty.");
    }

    public static void ApplyBearerToken(HttpClient client, string accessToken)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    public static void ClearBearerToken(HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = null;
    }
}
