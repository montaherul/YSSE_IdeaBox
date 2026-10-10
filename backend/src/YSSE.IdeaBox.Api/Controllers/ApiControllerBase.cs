using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YSSE.IdeaBox.Api.Common;
using YSSE.IdeaBox.Application.Common;

namespace YSSE.IdeaBox.Api.Controllers;

/// <summary>
/// Thin base for API controllers. Provides the standard success/error envelopes and
/// the correlation id; it holds no business logic.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected string TraceId => HttpContext.TraceIdentifier;

    protected IActionResult OkData<T>(T data) => Ok(ApiResponse<T>.Ok(data, TraceId));

    protected IActionResult OkResult(Result result) =>
        result.Success ? Ok(ApiResponse<object>.Ok(new { message = result.Message }, TraceId))
                       : Failed(result.Message);

    protected IActionResult OkResult<T>(Result<T> result) =>
        result.Success ? Ok(ApiResponse<T>.Ok(result.Data!, TraceId)) : Failed(result.Message);

    protected IActionResult Paged<T>(PagedResult<T> paged) => Ok(ApiResults.FromPaged(paged, TraceId));

    protected IActionResult Failed(string? message, string code = "REQUEST_FAILED") =>
        BadRequest(ApiResults.Error(message, code, TraceId));

    protected IActionResult NotFoundError(string? message = "Resource not found.") =>
        NotFound(ApiResults.Error(message, "NOT_FOUND", TraceId));

    protected IActionResult ForbiddenError(string? message) =>
        StatusCode(StatusCodes.Status403Forbidden, ApiResults.Error(message, "FORBIDDEN", TraceId));
}
