using System.Net.Http.Headers;
using System.Text;
using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Domain.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;

namespace DrugsApp.IntegrationTests.Common;

public sealed class DrugsAppWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _postgresConnectionString;
    private readonly string _redisConnectionString;

    public CapturingEmailVerificationSender EmailSender { get; } = new();

    public DrugsAppWebApplicationFactory(string postgresConnectionString, string redisConnectionString)
    {
        _postgresConnectionString = postgresConnectionString;
        _redisConnectionString = redisConnectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = _postgresConnectionString,
                ["ConnectionStrings:Redis"] = _redisConnectionString,
                ["ConnectionStrings:BlobStorage"] = TestCredentials.AzuriteBlobConnectionString,
                ["AllowedOrigins:0"] = "http://localhost",
                ["Auth:Jwt:Issuer"] = "DrugsApp",
                ["Auth:Jwt:Audience"] = "DrugsApp.Client",
                ["Auth:Jwt:SecretKey"] = TestCredentials.TestJwtSecret,
                ["Auth:SuperAdmin:Email"] = TestCredentials.AdminEmail,
                ["Auth:SuperAdmin:Password"] = TestCredentials.AdminPassword,
                ["DevSeed:Enabled"] = "true",
                ["DevSeed:SeedKey"] = "integration_test_v1",
                ["Notifications:WeeklyHealthDataReminder:Enabled"] = "false",
                ["RegularDrugs:ExpirationJob:Enabled"] = "false",
                ["RegularFood:ExpirationJob:Enabled"] = "false",
                ["RegularSupplements:ExpirationJob:Enabled"] = "false",
                ["Email:SenderEmail"] = "no-reply@test.local",
                ["Email:SenderName"] = "DrugsApp Tests",
                ["Email:SmtpHost"] = "localhost",
                ["Email:SmtpPort"] = "2525",
                ["Hangfire:DashboardPath"] = "/hangfire",
                ["Hangfire:EnableDashboard"] = "false",
                ["Serilog:WriteTo:0:Name"] = "Console",
                ["Serilog:MinimumLevel:Default"] = "Warning"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(TestCredentials.TestJwtSecret));
                options.TokenValidationParameters.ValidIssuer = "DrugsApp";
                options.TokenValidationParameters.ValidAudience = "DrugsApp.Client";
            });

            services.RemoveAll<IEmailVerificationSender>();
            services.AddSingleton(EmailSender);
            services.AddSingleton<IEmailVerificationSender>(sp => sp.GetRequiredService<CapturingEmailVerificationSender>());

            services.RemoveAll<IBlobService>();
            services.AddSingleton<InMemoryBlobService>();
            services.AddSingleton<IBlobService>(sp => sp.GetRequiredService<InMemoryBlobService>());

            services.AddHttpClient("ImportDownload")
                .ConfigurePrimaryHttpMessageHandler(() =>
                    new TempDrugImportDownloadHandler(TempDrugTestExcel.CreateSampleWorkbookBytes()));
        });
    }
}
