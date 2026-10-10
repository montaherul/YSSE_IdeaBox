namespace YSSE.IdeaBox.Application.Common;

/// <summary>Lightweight operation result used by services returning success/failure with a message.</summary>
public readonly record struct Result(bool Success, string? Message)
{
    public static Result Ok(string? message = null) => new(true, message);

    public static Result Fail(string message) => new(false, message);
}

public readonly record struct Result<T>(bool Success, T? Data, string? Message)
{
    public static Result<T> Ok(T data, string? message = null) => new(true, data, message);

    public static Result<T> Fail(string message) => new(false, default, message);
}

/// <summary>Server-side pagination envelope. Default page size 20, max 100 (NFR-03).</summary>
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();

    public int Page { get; init; }

    public int PageSize { get; init; }

    public int TotalCount { get; init; }

    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasNext => Page < TotalPages;

    public bool HasPrevious => Page > 1;

    public static PagedResult<T> Create(IReadOnlyList<T> items, int page, int pageSize, int total) =>
        new() { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
}

/// <summary>Normalizes client pagination input so list endpoints stay bounded.</summary>
public static class Pagination
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public static (int Page, int PageSize) Normalize(int? page, int? pageSize)
    {
        var p = page is null or < 1 ? 1 : page.Value;
        var s = pageSize is null or < 1 ? DefaultPageSize : Math.Min(pageSize.Value, MaxPageSize);
        return (p, s);
    }
}
