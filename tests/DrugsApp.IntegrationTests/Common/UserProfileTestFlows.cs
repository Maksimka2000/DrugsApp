using System.Net.Http.Headers;
using DrugsApp.Features.AuthFeature.LoginMember;
using DrugsApp.Features.AuthFeature.RegisterMember;

namespace DrugsApp.IntegrationTests.Common;

internal static class UserProfileTestFlows
{
    internal static async Task AuthenticateAsClient1Async(
        HttpClient client,
        CancellationToken cancellationToken)
    {
        LoginMemberResponse login = await TestAuthHelper.LoginAsClientAsync(client, cancellationToken);
        TestAuthHelper.ApplyBearerToken(client, login.AccessToken);
    }

    internal static async Task AuthenticateAsProvider1Async(
        HttpClient client,
        CancellationToken cancellationToken)
    {
        LoginMemberResponse login = await TestAuthHelper.LoginAsProviderAsync(client, cancellationToken);
        TestAuthHelper.ApplyBearerToken(client, login.AccessToken);
    }

    internal static async Task<AuthenticatedMember> RegisterConfirmAndLoginAsync(
        HttpClient client,
        CapturingEmailVerificationSender emailSender,
        CancellationToken cancellationToken)
    {
        const string password = "UserProfileTest123!";
        string email = IntegrationAuthFlows.CreateUniqueEmail();

        RegisterMemberResponse registration = await IntegrationAuthFlows.RegisterAndConfirmMemberAsync(
            client,
            emailSender,
            email,
            password,
            cancellationToken);

        return await LoginMemberAsync(client, email, password, registration.UserId, cancellationToken);
    }

    internal static async Task<AuthenticatedMember> LoginMemberAsync(
        HttpClient client,
        string email,
        string password,
        Guid userId,
        CancellationToken cancellationToken)
    {
        LoginMemberResponse login = await TestAuthHelper.LoginMemberAsync(
            client,
            email,
            password,
            cancellationToken);

        return new AuthenticatedMember(userId, email, password, login.AccessToken);
    }

    internal static async Task<HttpResponseMessage> UploadProfilePhotoAsync(
        HttpClient client,
        byte[] photoBytes,
        CancellationToken cancellationToken)
    {
        using var content = new MultipartFormDataContent();
        ByteArrayContent fileContent = new(photoBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(fileContent, "File", "profile.png");

        return await client.PostAsync("/api/user-profile/photo", content, cancellationToken);
    }

    internal static byte[] CreateMinimalPngBytes() =>
        Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");
}

internal sealed record AuthenticatedMember(
    Guid UserId,
    string Email,
    string Password,
    string AccessToken);
