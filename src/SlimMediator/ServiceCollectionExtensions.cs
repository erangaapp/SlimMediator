using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SlimMediator;

/// <summary>DI registration for SlimMediator.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="ISender"/> and every <see cref="IRequestHandler{TRequest, TResponse}"/> found in the
    /// configured assemblies, plus any open behaviors, in the order they were added.
    /// </summary>
    public static IServiceCollection AddSlimMediator(
        this IServiceCollection services,
        Action<SlimMediatorOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new SlimMediatorOptions();
        configure(options);

        services.TryAddScoped<ISender, Sender>();

        RegisterHandlers(services, options);

        foreach (var behavior in options.OpenBehaviors)
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), behavior);
        }

        return services;
    }

    /// <summary>
    /// Shorthand: registers <see cref="ISender"/> and scans <paramref name="assemblies"/> for handlers.
    /// Add behaviors afterwards with <see cref="AddPipelineBehavior"/>.
    /// </summary>
    public static IServiceCollection AddSlimMediator(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);

        return services.AddSlimMediator(cfg =>
        {
            foreach (var assembly in assemblies)
            {
                cfg.RegisterServicesFromAssembly(assembly);
            }
        });
    }

    /// <summary>
    /// Registers an open-generic behavior such as <c>typeof(LoggingBehavior&lt;,&gt;)</c>.
    /// Behaviors run in registration order; the first registered is outermost.
    /// </summary>
    public static IServiceCollection AddPipelineBehavior(
        this IServiceCollection services,
        Type openBehaviorType)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(openBehaviorType);

        if (!openBehaviorType.IsGenericTypeDefinition)
        {
            throw new ArgumentException(
                $"'{openBehaviorType}' must be an open generic type, e.g. typeof(LoggingBehavior<,>).",
                nameof(openBehaviorType));
        }

        services.AddTransient(typeof(IPipelineBehavior<,>), openBehaviorType);
        return services;
    }

    private static void RegisterHandlers(IServiceCollection services, SlimMediatorOptions options)
    {
        var handlerInterface = typeof(IRequestHandler<,>);

        var candidates = options.Assemblies
            .Distinct()
            .SelectMany(a => a.GetTypes())
            .Where(t => t is { IsAbstract: false, IsInterface: false, IsGenericTypeDefinition: false });

        foreach (var implementation in candidates)
        {
            var closedInterfaces = implementation.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterface);

            foreach (var closed in closedInterfaces)
            {
                services.AddTransient(closed, implementation);
            }
        }
    }
}