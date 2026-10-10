using YSSE.IdeaBox.Application.Common;

namespace YSSE.IdeaBox.Api.Common;

/// <summary>
/// Maps application-layer results to the standard HTTP envelopes.
/// Failed <see cref="Result"/> values become a 400 error envelope; thrown
/// <see cref="AppException"/>s are handled centrally by the exception middleware.
/// </summary>
public static class ApiResults
{
    public static ApiResponse<T> Ok<T>(T data, string? traceId) =>
        ApiResponse<T>.Ok(data, traceId);

    public static object FromPaged<T>(PagedResult<T> paged, string? traceId) => new PagedApiResponse<T>
    {
        Success = true,
        TraceId = traceId,
        Data = new PagedData<T>
        {
            Items = paged.Items,
            Page = paged.Page,
            PageSize = paged.PageSize,
            Total = paged.TotalCount,
            HasNextPage = paged.HasNext
        }
    };

    public static ApiErrorResponse Error(string? message, string code, string? traceId,
        IReadOnlyDictionary<string, string[]>? details = null) => new()
    {
        Success = false,
        TraceId = traceId,
        Error = new ApiError
        {
            Message = string.IsNullOrWhiteSpace(message) ? "The request could not be completed." : message!,
            Code = code,
            Details = details
        }
    };
}
