using DrugsApp.Features.AuthFeature.LoginAdmin;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.AuthFeature.LoginAdmin;

public sealed class LoginAdminReturns200Tests : IntegrationTestBase
{
    [Fact]
    public async Task LoginAdmin_WithSuperAdmin_Returns200AndTokens()
    {
        LoginAdminResponse response = await TestAuthHelper.LoginAsAdminAsync(Client, CancellationToken.None);

        response.AccessToken.Should().NotBeNullOrWhiteSpace();
        response.RefreshToken.Should().NotBeNullOrWhiteSpace();
        response.ExpiresInSeconds.Should().BeGreaterThan(0);
    }
}
