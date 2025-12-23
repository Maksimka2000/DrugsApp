using System.Reflection;
using DrugsApp.Features.Common.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DrugsApp.Features;

public static class DependencyInjection
{
    public static IServiceCollection AddFeatures(
        this IServiceCollection services)
    {
        Assembly assembly = typeof(DependencyInjection).Assembly;

        // Register MediatR
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(assembly);
            // Register pipeline behaviors (ORDER MATTERS!)
            configuration.AddOpenBehavior(typeof(RequestLoggingPipelineBehavior<,>));
            configuration.AddOpenBehavior(typeof(TransactionBehavior<,>));
        });


        // Register FluentValidation
        services.AddValidatorsFromAssembly(assembly);

        // Register applicaiton and domain services here

        return services;
    }
}

