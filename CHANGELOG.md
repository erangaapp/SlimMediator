# Changelog

## 1.0.0

- Initial release: `IRequest<T>`, `IRequest`, `Unit`, `IRequestHandler<,>`, `ISender`,
  `IPipelineBehavior<,>`, `RequestHandlerDelegate<T>`.
- `AddSlimMediator(cfg => ...)` with assembly scanning and `AddOpenBehavior`.
- `LoggingBehavior<,>` using `[LoggerMessage]` source generation.
- Per-request-type wrapper cache: no reflection or `dynamic` on the hot path.
