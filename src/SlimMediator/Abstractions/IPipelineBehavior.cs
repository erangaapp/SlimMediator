namespace SlimMediator;

/// <summary>Continues the pipeline: the next behavior, or the handler itself.</summary>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

/// <summary>
/// Wraps every request of type <typeparamref name="TRequest"/>. Behaviors run in registration order:
/// the first registered is the outermost.
/// </summary>
public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>Runs before/after <paramref name="next"/>. Skip calling <paramref name="next"/> to short-circuit.</summary>
    Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
}