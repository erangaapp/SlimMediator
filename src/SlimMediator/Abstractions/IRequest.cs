namespace SlimMediator;

/// <summary>Marks a command or query that produces a <typeparamref name="TResponse"/>.</summary>
/// <typeparam name="TResponse">The type returned by the handler.</typeparam>
#pragma warning disable CA1040 // Marker interface is the whole point.
public interface IRequest<out TResponse>;
#pragma warning restore CA1040

/// <summary>Marks a command that produces no meaningful result (returns <see cref="Unit"/>).</summary>
public interface IRequest : IRequest<Unit>;