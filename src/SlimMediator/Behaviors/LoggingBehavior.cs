using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace SlimMediator.Behaviors;

/// <summary>Logs the start, duration and failure of every request. Add with <c>AddOpenBehavior(typeof(LoggingBehavior&lt;,&gt;))</c>.</summary>
public sealed partial class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    /// <summary>Creates the behavior.</summary>
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var name = typeof(TRequest).Name;
        var start = Stopwatch.GetTimestamp();
        LogStarted(name);
        try
        {
            var response = await next().ConfigureAwait(false);
            LogCompleted(name, Stopwatch.GetElapsedTime(start).TotalMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            LogFailed(ex, name, Stopwatch.GetElapsedTime(start).TotalMilliseconds);
            throw;
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Handling {Request}")]
    private partial void LogStarted(string request);

    [LoggerMessage(Level = LogLevel.Information, Message = "Handled {Request} in {ElapsedMs:F1} ms")]
    private partial void LogCompleted(string request, double elapsedMs);

    [LoggerMessage(Level = LogLevel.Error, Message = "{Request} failed after {ElapsedMs:F1} ms")]
    private partial void LogFailed(Exception exception, string request, double elapsedMs);
}