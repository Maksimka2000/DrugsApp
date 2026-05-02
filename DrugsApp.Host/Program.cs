using Microsoft.AspNetCore.Builder;
using DrugsApp.Domain.TempDrugFeature.Interfaces;
using DrugsApp.Features;
using DrugsApp.Infrastructure;
using DrugsApp.Host.Exceptions;
using DrugsApp.Host.Hangfire;
using Serilog;
using DrugsApp.Infrastructure.Extensions;
using DrugsApp.Infrastructure.Extensions.Middleware;
using Microsoft.AspNetCore.Mvc;

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

// CORS
string[] allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()));

// Add Controllers
builder.Services.AddControllers();
// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline. Configure middleware
// Enable Swagger and migrations for non-Production environments (Development, Docker, Staging)
if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.ApplyMigrations();
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
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();

