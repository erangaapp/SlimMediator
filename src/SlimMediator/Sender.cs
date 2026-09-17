using System.Collections.Concurrent;
using SlimMediator.Internal;

namespace SlimMediator;

/// <summary>
/// Default <see cref="ISender"/>. Registered as a scoped service so handlers and behaviors
/// resolve from the caller's DI scope (DbContexts, current user, etc.).
/// </summary>
public sealed class Sender : ISender
{
    // Shared across all Sender instances: the wrapper holds no per-scope state,
    // so building it once per request type for the process lifetime is safe.
    private static readonly ConcurrentDictionary<Type, object> Wrappers = new();

    private readonly IServiceProvider _serviceProvider;

    /// <summary>Creates a sender bound to the current DI scope.</summary>
    public Sender(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <inheritdoc />
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var wrapper = (RequestHandlerBase<TResponse>)Wrappers.GetOrAdd(
            request.GetType(),
            static (requestType, responseType) =>
            {
                var wrapperType = typeof(RequestHandlerWrapper<,>).MakeGenericType(requestType, responseType);
                return Activator.CreateInstance(wrapperType)
                    ?? throw new InvalidOperationException($"Could not create wrapper for '{requestType}'.");
            },
            typeof(TResponse));

        return wrapper.Handle(request, _serviceProvider, cancellationToken);
    }
}