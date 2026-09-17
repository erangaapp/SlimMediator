# SlimMediator

A minimal, dependency-free in-process mediator for .NET. It implements the three
abstractions most services actually use from MediatR — `IRequest<T>`, `ISender`
and `IPipelineBehavior<,>` — in about 200 lines of library code, with no reflection
or `dynamic` on the hot path.

Built as a drop-in replacement when MediatR moved to a commercial license.
Existing handlers migrate with a `using` change.

## Install

```bash
dotnet add package SlimMediator
```

## Usage

```csharp
// 1. Define a request and its handler
public sealed record GetStudent(int Id) : IRequest<Student?>;

public sealed class GetStudentHandler : IRequestHandler<GetStudent, Student?>
{
    public Task<Student?> Handle(GetStudent request, CancellationToken ct) => ...;
}

// 2. Register: scan for handlers, add behaviors (first added = outermost)
builder.Services.AddSlimMediator(cfg => cfg
    .RegisterServicesFromAssemblyContaining<Program>()
    .AddOpenBehavior(typeof(LoggingBehavior<,>))
    .AddOpenBehavior(typeof(ValidationBehavior<,>)));

// 3. Send
app.MapGet("/students/{id:int}", (int id, ISender sender, CancellationToken ct)
    => sender.Send(new GetStudent(id), ct));
```

Commands without a result implement `IRequest` and return `Unit.Task`.

Shorter registration form, if you prefer:

```csharp
builder.Services.AddSlimMediator(typeof(Program).Assembly);
builder.Services.AddPipelineBehavior(typeof(LoggingBehavior<,>));
```

## Writing a behavior

```csharp
public sealed class TransactionBehavior<TRequest, TResponse>(AppDbContext db)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var response = await next();
        await tx.CommitAsync(ct);
        return response;
    }
}
```

Skip calling `next()` to short-circuit (e.g. return a cached response).

## How dispatch works

```
Send(request)
  └─ Wrappers[request.GetType()]           one RequestHandlerWrapper<TRequest,TResponse> per type, cached for the process
       ├─ resolve IRequestHandler<TRequest,TResponse> from the current scope
       ├─ resolve IPipelineBehavior<TRequest,TResponse>[] from the current scope
       └─ behavior[0]( behavior[1]( ... handler.Handle(request) ) )
```

The wrapper is created once per request type via `MakeGenericType` and cached in a
`ConcurrentDictionary`. Every call after the first is a dictionary lookup and a
virtual call — no reflection, no `dynamic`.

`ISender` is registered **scoped**, so handlers and behaviors resolve from the
caller's scope (per-request `DbContext`, current user, etc.).

## Migrating from MediatR

| MediatR | SlimMediator |
|---|---|
| `IRequest<T>`, `IRequest` | same |
| `IRequestHandler<TReq, TRes>` | same |
| `ISender.Send` | same |
| `IPipelineBehavior<TReq, TRes>` | same |
| `RequestHandlerDelegate<T>` | same |
| `Unit` | same |
| `AddMediatR(cfg => cfg.RegisterServicesFromAssembly(...))` | `AddSlimMediator(cfg => cfg.RegisterServicesFromAssembly(...))` |
| `cfg.AddOpenBehavior(typeof(X<,>))` | same |
| `INotification` / `IPublisher` | not included (by design) |
| `IStreamRequest` | not included |
| `IRequestPreProcessor` / `PostProcessor` | write a behavior instead |

For most codebases the migration is `using MediatR;` → `using SlimMediator;`.

## What is deliberately left out

Notifications, streams, pre/post processors and exception handlers. If you need
them, MediatR (or a fork) is the right tool. The value of this library is that it
is small enough to read in one sitting and fully own.

## Repository layout

```
src/SlimMediator/          the library (packable)
tests/SlimMediator.Tests/  xUnit tests: dispatch, ordering, short-circuit, missing handler
samples/SlimMediator.Sample/  console app showing queries, commands and behaviors
.github/workflows/         CI on every push; publish to nuget.org on a v* tag
```

## Publishing your own build

```bash
dotnet pack src/SlimMediator/SlimMediator.csproj -c Release -p:Version=1.0.0
dotnet nuget push artifacts/SlimMediator.1.0.0.nupkg --api-key <KEY> --source https://api.nuget.org/v3/index.json
```

Or push a tag (`git tag v1.0.0 && git push --tags`) and let `release.yml` do it —
it needs a `NUGET_API_KEY` repository secret.

## License

MIT
