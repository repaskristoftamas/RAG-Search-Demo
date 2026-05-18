using System.Reflection;
using FluentValidation;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace BookStore.Catalog.Application;

/// <summary>
/// Registers application-layer services into the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Mediator, FluentValidation validators, and application-layer services to the service collection.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
        });

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}
