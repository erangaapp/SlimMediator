using System.Reflection;

namespace SlimMediator;

/// <summary>Configuration collected by <see cref="ServiceCollectionExtensions.AddSlimMediator"/>.</summary>
public sealed class SlimMediatorOptions
{
    internal List<Assembly> Assemblies { get; } = [];
    internal List<Type> OpenBehaviors { get; } = [];

    /// <summary>Scan <paramref name="assembly"/> for <see cref="IRequestHandler{TRequest, TResponse}"/> implementations.</summary>
    public SlimMediatorOptions RegisterServicesFromAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        Assemblies.Add(assembly);
        return this;
    }

    /// <summary>Scan the assembly that contains <typeparamref name="TMarker"/>.</summary>
    public SlimMediatorOptions RegisterServicesFromAssemblyContaining<TMarker>()
        => RegisterServicesFromAssembly(typeof(TMarker).Assembly);

    /// <summary>
    /// Register an open-generic behavior such as <c>typeof(LoggingBehavior&lt;,&gt;)</c>.
    /// Behaviors run in the order they are added; the first added is outermost.
    /// </summary>
    public SlimMediatorOptions AddOpenBehavior(Type openBehaviorType)
    {
        ArgumentNullException.ThrowIfNull(openBehaviorType);
        if (!openBehaviorType.IsGenericTypeDefinition)
        {
            throw new ArgumentException(
                $"'{openBehaviorType}' must be an open generic type, e.g. typeof(LoggingBehavior<,>).",
                nameof(openBehaviorType));
        }

        OpenBehaviors.Add(openBehaviorType);
        return this;
    }
}