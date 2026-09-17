namespace SlimMediator;

/// <summary>Handles a single <typeparamref name="TRequest"/>. Exactly one handler must be registered per request type.</summary>
public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>Executes the request.</summary>
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}

/// <summary>Convenience base for handlers of <see cref="IRequest"/> (no result).</summary>
public interface IRequestHandler<in TRequest> : IRequestHandler<TRequest, Unit>
    where TRequest : IRequest;