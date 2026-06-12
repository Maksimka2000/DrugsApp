using DrugsApp.Infrastructure;
using DrugsApp.Infrastructure.Persistence;
using DrugsApp.Infrastructure.Services.AuthFeature;
using DrugsApp.Infrastructure.Services.Seed;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"), optional: true)
    .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.Development.json"), optional: false)
    .AddEnvironmentVariables();

builder.Services.AddLogging(logging => logging.AddConsole());
builder.Services.AddDataProtection();
builder.Services.AddInfrastructure(builder.Configuration);

IHost host = builder.Build();

using IServiceScope scope = host.Services.CreateScope();
ILogger<Program> logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

DrugsAppDbContext dbContext = scope.ServiceProvider.GetRequiredService<DrugsAppDbContext>();
await dbContext.Database.MigrateAsync();

IdentityAuthorizationDataSeeder identitySeeder =
    scope.ServiceProvider.GetRequiredService<IdentityAuthorizationDataSeeder>();
await identitySeeder.SeedAsync(CancellationToken.None);

PatientDevDataSeeder patientSeeder = scope.ServiceProvider.GetRequiredService<PatientDevDataSeeder>();
await patientSeeder.SeedAsync(CancellationToken.None);

logger.LogInformation("Dev seed runner finished.");

internal partial class Program;
