namespace YSSE.IdeaBox.Api.Common;

/// <summary>Standard success envelope consumed by the SPA (see api-client.js).</summary>
public class ApiResponse<T>
{
    public bool Success { get; init; }

    public T? Data { get; init; }

    public string? TraceId { get; init; }

    public static ApiResponse<T> Ok(T data, string? traceId = null) =>
        new() { Success = true, Data = data, TraceId = traceId };
}

/// <summary>
/// Paged payload. Nested under <see cref="ApiResponse{T}.Data"/> so the SPA can read
/// <c>data.items</c> together with the paging metadata after it unwraps the envelope.
/// </summary>
public class PagedData<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();

    public int Page { get; init; }

    public int PageSize { get; init; }

    public int Total { get; init; }

    public bool HasNextPage { get; init; }
}

public class PagedApiResponse<T>
{
    public bool Success { get; init; }

    public PagedData<T> Data { get; init; } = new();

    public string? TraceId { get; init; }
}
