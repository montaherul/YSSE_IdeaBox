namespace YSSE.IdeaBox.Api.Common;

/// <summary>Machine-readable error payload returned for every failed request.</summary>
public class ApiError
{
    public string Message { get; init; } = string.Empty;

    public string Code { get; init; } = "ERROR";

    public IReadOnlyDictionary<string, string[]>? Details { get; init; }
}

public class ApiErrorResponse
{
    public bool Success { get; init; }

    public ApiError Error { get; init; } = new();

    public string? TraceId { get; init; }
}
