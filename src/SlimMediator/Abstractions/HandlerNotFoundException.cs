using SlimMediator;


/// <summary>Thrown when no <see cref="IRequestHandler{TRequest,TResponse}"/> is registered for a request.</summary>
public sealed class HandlerNotFoundException(Type requestType)
    : InvalidOperationException(
        $"No handler registered for request '{requestType.FullName}'. " +
        $"Register an IRequestHandler<{requestType.Name}, TResponse> or call AddSlimMediator(assembly) with the assembly that contains it.")
{
    /// <summary>The request type that had no registered handler.</summary>
    public Type RequestType { get; } = requestType;
}