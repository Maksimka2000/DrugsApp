using Microsoft.AspNetCore.Builder;
using DrugsApp.Features;
using DrugsApp.Infrastructure;
using Serilog;
using DrugsApp.Infrastructure.Extensions;
using DrugsApp.Infrastructure.Extensions.Middleware;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));

// Register Infrastructure (DbContext, Repositories, UnitOfWork)
// Register Features (MediatR, FluentValidation, Behaviors)
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddFeatures();

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

// 
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLogContextMiddleware>();
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();

