using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SlimMediator;
using SlimMediator.Behaviors;
using SlimMediator.Sample;

var services = new ServiceCollection();
services.AddLogging(b => b.AddSimpleConsole(o => o.SingleLine = true).SetMinimumLevel(LogLevel.Information));

services.AddSlimMediator(cfg => cfg
    .RegisterServicesFromAssemblyContaining<Program>()
    .AddOpenBehavior(typeof(LoggingBehavior<,>))      // outermost
    .AddOpenBehavior(typeof(ValidationBehavior<,>))); // runs inside logging

using var provider = services.BuildServiceProvider();
using var scope = provider.CreateScope();
var sender = scope.ServiceProvider.GetRequiredService<ISender>();

var student = await sender.Send(new GetStudent(1));
Console.WriteLine($"Found: {student?.Name} ({student?.Programme})");

await sender.Send(new EnrolStudent(1, "CS301"));

try
{
    await sender.Send(new Missing());
}
catch (HandlerNotFoundException ex)
{
    Console.WriteLine($"Expected failure: {ex.Message}");
}

public sealed record Missing : IRequest<int>;