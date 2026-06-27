using DrugsApp.Infrastructure.Persistence;
using DrugsApp.Infrastructure.Services.AuthFeature;
using DrugsApp.Infrastructure.Services.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Xunit;

namespace DrugsApp.IntegrationTests.Common;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    private PostgreSqlContainer? _postgresContainer;
    private RedisContainer? _redisContainer;

    protected HttpClient Client { get; private set; } = null!;

    protected DrugsAppWebApplicationFactory Factory { get; private set; } = null!;

    protected CapturingEmailVerificationSender EmailSender => Factory.EmailSender;

    public async Task InitializeAsync()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .Build();

        _redisContainer = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .Build();

        await _postgresContainer.StartAsync();
        await _redisContainer.StartAsync();

        Factory = new DrugsAppWebApplicationFactory(
            _postgresContainer.GetConnectionString(),
            _redisContainer.GetConnectionString());

        Client = Factory.CreateClient();
        EmailSender.Clear();

        await EnsureDatabaseSeededAsync();
    }

    private async Task EnsureDatabaseSeededAsync()
    {
        using IServiceScope scope = Factory.Services.CreateScope();
        DrugsAppDbContext dbContext = scope.ServiceProvider.GetRequiredService<DrugsAppDbContext>();
        await dbContext.Database.MigrateAsync();

        IdentityAuthorizationDataSeeder identitySeeder =
            scope.ServiceProvider.GetRequiredService<IdentityAuthorizationDataSeeder>();
        await identitySeeder.SeedAsync(CancellationToken.None);

        await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM dev_seed_applications");

        PatientDevDataSeeder patientSeeder = scope.ServiceProvider.GetRequiredService<PatientDevDataSeeder>();
        await patientSeeder.SeedAsync(CancellationToken.None);
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await Factory.DisposeAsync();

        if (_postgresContainer is not null)
        {
            await _postgresContainer.DisposeAsync();
        }

        if (_redisContainer is not null)
        {
            await _redisContainer.DisposeAsync();
        }
    }
}
