namespace ClubIt.Application.Common;

public class NotFoundException(string message) : Exception(message);

public class ForbiddenException(string message) : Exception(message);

public class UnauthorizedException(string message) : Exception(message);

public class ConflictException(string message) : Exception(message);

public class ValidationException : Exception
{
    public ValidationException(IDictionary<string, string[]> errors)
        : base("אחד או יותר מהשדות אינם תקינים.")
    {
        Errors = errors;
    }

    public ValidationException(string field, string error)
        : this(new Dictionary<string, string[]> { [field] = [error] })
    {
    }

    public IDictionary<string, string[]> Errors { get; }
}
