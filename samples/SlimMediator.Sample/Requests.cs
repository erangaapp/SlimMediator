using SlimMediator;

namespace SlimMediator.Sample;

// --- A query with a result -------------------------------------------------

public sealed record Student(int Id, string Name, string Programme);

public sealed record GetStudent(int Id) : IRequest<Student?>;

public sealed class GetStudentHandler : IRequestHandler<GetStudent, Student?>
{
    private static readonly Dictionary<int, Student> Store = new()
    {
        [1] = new Student(1, "Amal", "BSc Computer Science"),
        [2] = new Student(2, "Nadia", "BA International Relations"),
    };

    public Task<Student?> Handle(GetStudent request, CancellationToken cancellationToken)
        => Task.FromResult(Store.GetValueOrDefault(request.Id));
}

// --- A command with no result -------------------------------------------

public sealed record EnrolStudent(int StudentId, string CourseCode) : IRequest;

public sealed class EnrolStudentHandler : IRequestHandler<EnrolStudent>
{
    public Task<Unit> Handle(EnrolStudent request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"  -> enrolled student {request.StudentId} in {request.CourseCode}");
        return Unit.Task;
    }
}