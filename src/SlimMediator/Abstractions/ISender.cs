namespace SlimMediator;

/// <summary>Dispatches a request to its single registered handler through the behavior pipeline.</summary>
public interface ISender
{
    /// <summary>Sends <paramref name="request"/> and returns the handler's response.</summary>
    /// <exception cref="HandlerNotFoundException">No handler is registered for the request type.</exception>
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}