namespace DrugsApp.IntegrationTests.Common;

internal static class TestCredentials
{
    internal static readonly Guid Client1Id = Guid.Parse("b1010000-0000-4000-8000-000000000001");

    internal static readonly Guid Provider1Id = Guid.Parse("b2010000-0000-4000-8000-000000000001");

    internal const string Client1Email = "seed.client1@drugsapp.local";
    internal const string Client1Password = "Client1Seed123!";

    internal const string Provider1Email = "seed.provider1@drugsapp.local";
    internal const string Provider1Password = "Provider1Seed123!";

    internal const string AdminEmail = "admin@drugsapp.local";
    internal const string AdminPassword = "Admin123!";

    internal const string TestJwtSecret = "integration-test-jwt-secret-at-least-32-chars";

    internal const string AzuriteBlobConnectionString =
        "AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";
}
