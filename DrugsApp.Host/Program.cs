using System.Text;
using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Domain.TempDrugFeature.Interfaces;
using DrugsApp.Features;
using DrugsApp.Host.Auth.Authorization;
using DrugsApp.Host.Auth.Jwt;
using DrugsApp.Host.Auth.Swagger;
using DrugsApp.Host.Exceptions;
using DrugsApp.Host.Hangfire;
using DrugsApp.Host.Hangfire.NotificationsFeature;
using DrugsApp.Host.Hangfire.RegularDrugsFeature;
using DrugsApp.Host.Hangfire.RegularFoodFeature;
using DrugsApp.Host.Hangfire.RegularSupplementsFeature;
using Hangfire;
using DrugsApp.Infrastructure;
using DrugsApp.Infrastructure.Extensions;
using DrugsApp.Infrastructure.Extensions.Middleware;
using DrugsApp.Infrastructure.Services.AuthFeature;
using DrugsApp.Infrastructure.Services.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry();
// Configure Serilog
builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));

// Register Infrastructure (DbContext, Repositories, UnitOfWork)
builder.Services.AddInfrastructure(builder.Configuration);
// Register Features (MediatR, FluentValidation, Behaviors)
builder.Services.AddFeatures();
// Register Hangfire (Storage, retries, worker server)
builder.Services.AddHangfireProcessing(builder.Configuration);
builder.Services.Configure<WeeklyHealthDataReminderJobSettings>(
    builder.Configuration.GetSection(WeeklyHealthDataReminderJobSettings.SectionName));
builder.Services.Configure<RegularDrugExpirationJobSettings>(
    builder.Configuration.GetSection(RegularDrugExpirationJobSettings.SectionName));
builder.Services.Configure<RegularFoodExpirationJobSettings>(
    builder.Configuration.GetSection(RegularFoodExpirationJobSettings.SectionName));
builder.Services.Configure<RegularSupplementExpirationJobSettings>(
    builder.Configuration.GetSection(RegularSupplementExpirationJobSettings.SectionName));
builder.Services.AddScoped<ITempDrugImportJobScheduler, HangfireTempDrugImportJobScheduler>();
builder.Services.AddScoped<ITempDrugImportJobStatusProvider, HangfireTempDrugImportJobStatusProvider>();

// Exception Handling
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Authentication and Authorization

// JWT Settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
JwtSettings jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new();
if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
{
    throw new InvalidOperationException("Auth:Jwt:SecretKey must be configured.");
}
// JwtTokenProvider registration
builder.Services.AddScoped<IJwtTokenProvider, JwtTokenProvider>();
// Authentication configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = "sub"
        };
    });

builder.Services.AddAuthorization();
// create custom authorization policy provider and handlers. PolicyProvider builds policy. 
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
// they enforce the policies that PermissionAuthorizationPolicyProvider creates.
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, AnyPermissionAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, AnyPermissionOrClientRoleAuthorizationHandler>();

// CORS
string[] allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()));

// Add Controllers
builder.Services.AddControllers(options =>
{
        // Apply authorization conventions to controllers 
    options.Conventions.Add(new ApiAuthorizationConvention());
});
// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerJwt();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline. Configure middleware
// Enable Swagger and migrations for non-Production environments (Development, Docker, Staging)
if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.ApplyMigrations();
 
    // Seed roles, permission claims, and configured super admin (non-production only)
    using IServiceScope scope = app.Services.CreateScope();
    IdentityAuthorizationDataSeeder seeder = scope.ServiceProvider.GetRequiredService<IdentityAuthorizationDataSeeder>();
    await seeder.SeedAsync(CancellationToken.None);

    PatientDevDataSeeder patientDevDataSeeder = scope.ServiceProvider.GetRequiredService<PatientDevDataSeeder>();
    await patientDevDataSeeder.SeedAsync(CancellationToken.None);
}

// Exception Handling
app.UseExceptionHandler();
// CORS must precede HTTPS redirection so preflight responses include the correct headers
app.UseCors();
// Logging
app.UseMiddleware<RequestLogContextMiddleware>();
app.UseSerilogRequestLogging();
// Hangfire dashboard
app.UseConfiguredHangfireDashboard();
// Routing
app.UseHttpsRedirection();
// Authentication
app.UseAuthentication();
// Authorization
app.UseAuthorization();
app.MapControllers();

WeeklyHealthDataReminderJobSettings weeklyHealthReminderSettings = app.Services
    .GetRequiredService<IOptions<WeeklyHealthDataReminderJobSettings>>()
    .Value;
if (weeklyHealthReminderSettings.Enabled)
{
    RecurringJob.AddOrUpdate<WeeklyHealthDataReminderHangfireJob>(
        "weekly-health-data-reminder",
        job => job.ExecuteAsync(CancellationToken.None),
        weeklyHealthReminderSettings.CronExpression);
}

RegularDrugExpirationJobSettings regularDrugExpirationSettings = app.Services
    .GetRequiredService<IOptions<RegularDrugExpirationJobSettings>>()
    .Value;
if (regularDrugExpirationSettings.Enabled)
{
    RecurringJob.AddOrUpdate<RegularDrugExpirationHangfireJob>(
        "regular-drug-expiration",
        job => job.ExecuteAsync(CancellationToken.None),
        regularDrugExpirationSettings.CronExpression);
}

RegularFoodExpirationJobSettings regularFoodExpirationSettings = app.Services
    .GetRequiredService<IOptions<RegularFoodExpirationJobSettings>>()
    .Value;
if (regularFoodExpirationSettings.Enabled)
{
    RecurringJob.AddOrUpdate<RegularFoodExpirationHangfireJob>(
        "regular-food-expiration",
        job => job.ExecuteAsync(CancellationToken.None),
        regularFoodExpirationSettings.CronExpression);
}

RegularSupplementExpirationJobSettings regularSupplementExpirationSettings = app.Services
    .GetRequiredService<IOptions<RegularSupplementExpirationJobSettings>>()
    .Value;
if (regularSupplementExpirationSettings.Enabled)
{
    RecurringJob.AddOrUpdate<RegularSupplementExpirationHangfireJob>(
        "regular-supplement-expiration",
        job => job.ExecuteAsync(CancellationToken.None),
        regularSupplementExpirationSettings.CronExpression);
}

await app.RunAsync();

