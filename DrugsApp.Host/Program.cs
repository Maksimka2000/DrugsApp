using System.Text;
using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Domain.TempDrugFeature.Interfaces;
using DrugsApp.Features;
using DrugsApp.Host.Auth.Authorization;
using DrugsApp.Host.Auth.Jwt;
using DrugsApp.Host.Auth.Swagger;
using DrugsApp.Host.Exceptions;
using DrugsApp.Host.Hangfire;
using DrugsApp.Infrastructure;
using DrugsApp.Infrastructure.Extensions;
using DrugsApp.Infrastructure.Extensions.Middleware;
using DrugsApp.Infrastructure.Services.AuthFeature;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));

// Register Infrastructure (DbContext, Repositories, UnitOfWork)
builder.Services.AddInfrastructure(builder.Configuration);
// Register Features (MediatR, FluentValidation, Behaviors)
builder.Services.AddFeatures();
// Register Hangfire (Storage, retries, worker server)
builder.Services.AddHangfireProcessing(builder.Configuration);
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

await app.RunAsync();

