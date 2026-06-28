using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using System.Collections.Generic;

namespace DrugsApp.Host.Auth.Swagger;

/// <summary>
/// Swagger extensions to register JWT bearer security definitions.
/// </summary>
public static class SwaggerJwtConfigurationExtensions
{
    /// <summary>
    /// Adds JWT bearer security definition and requirement to Swagger.
    /// </summary>
    public static IServiceCollection AddSwaggerJwt(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Provide a JWT bearer token."
            });

            // Swashbuckle 10 requires the document-scoped reference so Swagger UI sends Authorization.
            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
        });

        return services;
    }
}
