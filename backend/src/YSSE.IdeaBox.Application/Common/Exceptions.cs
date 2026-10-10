namespace YSSE.IdeaBox.Application.Common;

/// <summary>Base application exception; message is safe to surface to clients.</summary>
public class AppException : Exception
{
    public int StatusCode { get; }

    public AppException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }
}

public sealed class NotFoundException : AppException
{
    public NotFoundException(string message = "Resource not found.") : base(message, 404) { }
}

public sealed class ForbiddenException : AppException
{
    public ForbiddenException(string message = "You are not allowed to perform this action.") : base(message, 403) { }
}

public sealed class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "Authentication is required.") : base(message, 401) { }
}

public sealed class ConflictException : AppException
{
    public ConflictException(string message) : base(message, 409) { }
}

public sealed class ValidationAppException : AppException
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationAppException(IReadOnlyDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.", 400)
    {
        Errors = errors;
    }
}
