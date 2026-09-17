using Microsoft.Extensions.DependencyInjection;
using SlimMediator.Behaviors;
using Xunit;

namespace SlimMediator.Tests;

public sealed record Ping(string Message) : IRequest<string>;

public sealed class PingHandler : IRequestHandler<Ping, string>
{
    public Task<string> Handle(Ping request, CancellationToken cancellationToken)
        => Task.FromResult($"Pong: {request.Message}");
}

public sealed record Touch : IRequest;

public sealed class TouchHandler : IRequestHandler<Touch>
{
    public static int Calls;

    public Task<Unit> Handle(Touch request, CancellationToken cancellationToken)
    {
        Calls++;
        return Unit.Task;
    }
}

public sealed record Orphan : IRequest<int>;

public sealed class CallLog
{
    public List<string> Entries { get; } = [];
}

public sealed class FirstBehavior<TRequest, TResponse>(CallLog log) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        log.Entries.Add("first:before");
        var result = await next();
        log.Entries.Add("first:after");
        return result;
    }
}

public sealed class SecondBehavior<TRequest, TResponse>(CallLog log) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        log.Entries.Add("second:before");
        var result = await next();
        log.Entries.Add("second:after");
        return result;
    }
}

public sealed class ShortCircuitBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        => Task.FromResult((TResponse)(object)"short-circuited");
}

public class SenderTests
{
    private static ServiceProvider Build(Action<SlimMediatorOptions>? extra = null, Action<IServiceCollection>? services = null)
    {
        var sc = new ServiceCollection();
        sc.AddLogging();
        sc.AddSingleton<CallLog>();
        sc.AddSlimMediator(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<SenderTests>();
            extra?.Invoke(cfg);
        });
        services?.Invoke(sc);
        return sc.BuildServiceProvider();
    }

    [Fact]
    public async Task Send_dispatches_to_the_registered_handler()
    {
        using var sp = Build();
        var sender = sp.GetRequiredService<ISender>();

        var result = await sender.Send(new Ping("hello"));

        Assert.Equal("Pong: hello", result);
    }

    [Fact]
    public async Task Send_supports_unit_requests()
    {
        using var sp = Build();
        var sender = sp.GetRequiredService<ISender>();
        var before = TouchHandler.Calls;

        var result = await sender.Send(new Touch());

        Assert.Equal(Unit.Value, result);
        Assert.Equal(before + 1, TouchHandler.Calls);
    }

    [Fact]
    public async Task Send_throws_when_no_handler_is_registered()
    {
        using var sp = Build();
        var sender = sp.GetRequiredService<ISender>();

        var ex = await Assert.ThrowsAsync<HandlerNotFoundException>(() => sender.Send(new Orphan()));

        Assert.Equal(typeof(Orphan), ex.RequestType);
    }

    [Fact]
    public async Task Behaviors_run_in_registration_order_first_is_outermost()
    {
        using var sp = Build(cfg => cfg
            .AddOpenBehavior(typeof(FirstBehavior<,>))
            .AddOpenBehavior(typeof(SecondBehavior<,>)));
        var sender = sp.GetRequiredService<ISender>();
        var log = sp.GetRequiredService<CallLog>();

        await sender.Send(new Ping("x"));

        Assert.Equal(
            new[] { "first:before", "second:before", "second:after", "first:after" },
            log.Entries);
    }

    [Fact]
    public async Task Behavior_can_short_circuit_the_handler()
    {
        using var sp = Build(cfg => cfg.AddOpenBehavior(typeof(ShortCircuitBehavior<,>)));
        var sender = sp.GetRequiredService<ISender>();

        var result = await sender.Send(new Ping("never reached"));

        Assert.Equal("short-circuited", result);
    }

    [Fact]
    public async Task Logging_behavior_does_not_alter_the_response()
    {
        using var sp = Build(cfg => cfg.AddOpenBehavior(typeof(LoggingBehavior<,>)));
        var sender = sp.GetRequiredService<ISender>();

        var result = await sender.Send(new Ping("logged"));

        Assert.Equal("Pong: logged", result);
    }

    [Fact]
    public void AddOpenBehavior_rejects_closed_types()
    {
        var options = new SlimMediatorOptions();

        Assert.Throws<ArgumentException>(() => options.AddOpenBehavior(typeof(FirstBehavior<Ping, string>)));
    }
}