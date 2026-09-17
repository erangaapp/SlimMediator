using Microsoft.Extensions.DependencyInjection;

namespace SlimMediator.Internal;

/// <summary>
/// Non-generic entry point so <see cref="Sender"/> can cache one wrapper per request type
/// and call it without reflection or <c>dynamic</c> on the hot path.
/// </summary>
internal abstract class RequestHandlerBase<TResponse>
{
    public abstract Task<TResponse> Handle(
        IRequest<TResponse> request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

/// <summary>
/// Closed once per (TRequest, TResponse). Resolves the handler and behaviors from the current
/// scope and composes them into a single call chain, outermost behavior first.
/// </summary>
internal sealed class RequestHandlerWrapper<TRequest, TResponse> : RequestHandlerBase<TResponse>
    where TRequest : IRequest<TResponse>
{
    public override Task<TResponse> Handle(
        IRequest<TResponse> request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var handler = serviceProvider.GetService<IRequestHandler<TRequest, TResponse>>()
            ?? throw new HandlerNotFoundException(typeof(TRequest));

        var typedRequest = (TRequest)request;

        // Innermost call: the handler itself.
        RequestHandlerDelegate<TResponse> next = () => handler.Handle(typedRequest, cancellationToken);

        // Wrap from the inside out so the FIRST registered behavior ends up OUTERMOST.
        var behaviors = serviceProvider.GetServices<IPipelineBehavior<TRequest, TResponse>>().Reverse();
        foreach (var behavior in behaviors)
        {
            var inner = next;
            next = () => behavior.Handle(typedRequest, inner, cancellationToken);
        }

        return next();
    }
}