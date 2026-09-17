using SlimMediator;

namespace SlimMediator.Sample;

/// <summary>A tiny validation behavior: any request implementing IValidatable gets checked before its handler runs.</summary>
public interface IValidatable
{
    IEnumerable<string> Validate();
}

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is IValidatable v)
        {
            var errors = v.Validate().ToList();
            if (errors.Count > 0)
            {
                throw new InvalidOperationException("Validation failed: " + string.Join("; ", errors));
            }
        }

        return next();
    }
}