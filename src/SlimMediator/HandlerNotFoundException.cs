namespace SlimMediator;

/// <summary>Thrown when <see cref="ISender.Send{TResponse}"/> finds no handler for a request type.</summary>
public sealed class HandlerNotFoundException : InvalidOperationException
{
    /// <summary>Creates the exception for <paramref name="requestType"/>.</summary>
    public HandlerNotFoundException(Type requestType)
        : base($"No IRequestHandler is registered for '{requestType.FullName}'. " +
               "Register it with AddSlimMediator(cfg => cfg.RegisterServicesFromAssembly(...)) " +
               "or services.AddTransient<IRequestHandler<TRequest, TResponse>, THandler>().")
    {
        RequestType = requestType;
    }

    /// <inheritdoc />
    public HandlerNotFoundException() { }

    /// <inheritdoc />
    public HandlerNotFoundException(string message) : base(message) { }

    /// <inheritdoc />
    public HandlerNotFoundException(string message, Exception innerException) : base(message, innerException) { }

    /// <summary>The request type that had no handler, if known.</summary>
    public Type? RequestType { get; }
}