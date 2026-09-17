namespace SlimMediator;

/// <summary>A void-like return type for requests with no result.</summary>
public readonly record struct Unit
{
    /// <summary>The single <see cref="Unit"/> value.</summary>
    public static readonly Unit Value = default;

    /// <summary>A completed task carrying <see cref="Value"/>.</summary>
    public static readonly Task<Unit> Task = System.Threading.Tasks.Task.FromResult(Value);
}